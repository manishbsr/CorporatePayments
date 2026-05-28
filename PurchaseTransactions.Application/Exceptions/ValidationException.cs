

using FluentValidation.Results;

namespace PurchaseTransactions.Application.Exceptions
{
	public sealed class ValidationException : Exception
	{
		public ValidationException()
			: base("One or more validation failures have occurred.")
		{
		}

		public ValidationException(string message)
			: base($"One or more validation failures have occurred. {message}")
		{
		}
		public ValidationException(IList<ValidationFailure> failures)
				: this()
		{
			var propertyNames = failures
				.Select(e => e.PropertyName)
				.Distinct();

			foreach (var propertyName in propertyNames)
			{
				var propertyFailures = failures
					.Where(e => e.PropertyName == propertyName)
					.Select(e => e.ErrorMessage)
					.ToArray();

				Failures.Add(propertyName, propertyFailures);
			}
		}

		public ValidationException(IDictionary<string, string[]> failures)
			: this()
		{
			if (failures == null)
			{
				return;
			}

			foreach (var failure in failures)
			{
				Failures.Add(failure.Key, failure.Value);
			}
		}

		public ValidationException(string message, Exception innerException) : base(message, innerException)
		{
		}

		public IDictionary<string, string[]> Failures { get; } = new Dictionary<string, string[]>();
	}
}
