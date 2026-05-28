
using System.Runtime.CompilerServices;

namespace PurchaseTransactions.Application.Exceptions

{

	public sealed class MissingArgumentException : Exception
	{
		public MissingArgumentException()
		{
		}

		public MissingArgumentException(string message, Exception innerException) : base(message, innerException)
		{
		}

		public MissingArgumentException(string argumentName, [CallerMemberName] string? methodName = default)
			: base($"Argument '{argumentName}' is missing in call to '{methodName}'")
		{
		}
	}
}
