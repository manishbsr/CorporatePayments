using System.Net;

namespace PurchaseTransactions.Domain.Models
{
	public class PurchaseTransactionTargetCurrency : PurchaseTransaction
	{
		public decimal AmountWithTargetCurrent { get; set; }
		public string TargetCurrency { get; set; } = default!;
		public Error? Error { get; set; } 
	}
	public class Error
	{
		public HttpStatusCode Code { get; set; } = default!;
		public string Message { get; set; } = default!;
	}
}
