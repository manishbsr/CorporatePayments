
using FluentValidation;
using NLog;
using NLog.Web;
using PurchaseTransactions.API.Middleware;
using PurchaseTransactions.Application;
using PurchaseTransactions.Application.Services;

namespace PurchaseTransactions.API
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
			LogManager.Setup().LoadConfigurationFromFile($"nlog.{environment}.config", optional: true);
			LogManager.Setup().LoadConfigurationFromFile("nlog.config", optional: true);

			var builder = WebApplication.CreateBuilder(args);
			builder.Logging.ClearProviders();
			builder.Host.UseNLog();


			// Add services to the container.
			var connectionString = builder.Configuration.GetConnectionString("PurchaseTransactionsDb") ?? throw new InvalidOperationException("Connection string 'PurchaseTransactionsDb' not found.");
			builder.Services.AddInfrastructureServices(connectionString);
			builder.Services.AddApplicationServices();
			builder.Services.AddControllers()
					.AddJsonOptions(options =>
					{
						options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
						options.JsonSerializerOptions.Converters.Add(new DateTimeConvertor());

						options.AllowInputFormatterExceptionMessages = true;

					});
			builder.Services.AddValidatorsFromAssemblyContaining<SearchPurchaseTransactionsRequestValidator>();

			//Swagger
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen(options =>
			{
				options.SwaggerDoc("v1", new()
				{
					Version = "v1",
					Title = "Purchase Transactions API",
					Description = "API for managing purchase transactions."
				});
			});

			var app = builder.Build();

			using (var scope = app.Services.CreateScope())
			{
				var db = scope.ServiceProvider.GetRequiredService<Infrastructure.Persistence.AppDbContext>();
				db.Database.EnsureCreated();
			}

			app.UseMiddleware<ExceptionHandlingMiddleware>();
			app.UseSwagger();
			app.UseSwaggerUI(c =>
			{
				c.SwaggerEndpoint("/swagger/v1/swagger.json", "Purchase Transactions API V1");
				c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
			});


			app.UseHttpsRedirection();

			app.UseAuthorization();


			app.MapControllers();

			app.Run();
		}
	}
}
