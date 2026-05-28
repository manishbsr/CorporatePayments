namespace PurchaseTransactions.Application.Models
{
	public class CreatePurchaseTransactionRequest
	{
		public string Description { get; set; } = default!;
		public DateTime TransactionDate { get; set; }
		public decimal Amount { get; set; }
	}

	public class SearchPurchaseTransactionsRequest
	{
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public decimal? MinAmount { get; set; }
		public decimal? MaxAmount { get; set; }
		public string Currency { get; set; } = default!;
	}

	public class GetPurchaseTransactionRequest
	{
		public string Currency { get; set; } = default!;
		public string TransactionId { get; set; } = default!;
	}
}
