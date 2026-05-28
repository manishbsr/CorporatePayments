using PurchaseTransactions.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace PurchaseTransactions.API.Middleware
{
	public class ExceptionHandlingMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<ExceptionHandlingMiddleware> _logger;

		public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
		{
			_next = next ?? throw new ArgumentNullException(nameof(next));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}
		public async Task InvokeAsync(HttpContext context)
		{
			try
			{
				await _next(context);
			}
			catch (ValidationException ex)
			{
				_logger.LogError(ex, "Validation error while processing the request.");
				await WriteProblemDetailsAsync(context, StatusCodes.Status400BadRequest, "One or more validation errors occurred.", new Dictionary<string, object?> { ["errors"] = ex.Failures }, ex);
			}
			catch (MissingArgumentException ex)
			{
				_logger.LogError(ex, "Missing argument while processing the request.");
				await WriteProblemDetailsAsync(context, StatusCodes.Status400BadRequest, ex.Message ?? "A required argument was missing.", null, ex);
			}
			catch (NotFoundException ex)
			{
				_logger.LogError(ex, "Resource not found while processing the request.");
				await WriteProblemDetailsAsync(context, StatusCodes.Status404NotFound, ex.Message ?? "The requested resource was not found.", null, ex);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An unhandled exception occurred while processing the request.");
				await WriteProblemDetailsAsync(context, StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.", null, ex);
			}
		}

		private const string ProblemContentType = "application/problem+json";

		private static async Task WriteProblemDetailsAsync(HttpContext context, int statusCode, string title, IDictionary<string, object?>? extensions = null, Exception? ex = null)
		{
			context.Response.Clear();
			context.Response.StatusCode = statusCode;
			context.Response.ContentType = ProblemContentType;
			var problemDetails = new ProblemDetails
			{
				Type = $"https://httpstatuses.com/{statusCode}",
				Title = title,
				Status = statusCode,
				Instance = context.Request?.Path.Value,
				Detail = ex != null && !context.RequestServices.GetService<Microsoft.Extensions.Hosting.IHostEnvironment>()?.IsProduction() == true ? ex.Message : null
			};
			if (extensions != null)
			{
				foreach (var kvp in extensions)
				{
					if (kvp.Value != null)
					{
						problemDetails.Extensions[kvp.Key] = kvp.Value;
					}
				}
			}

			await context.Response.WriteAsJsonAsync(problemDetails);
		}
	}

}
