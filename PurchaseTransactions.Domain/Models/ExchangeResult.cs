namespace PurchaseTransactions.Domain.Models
{
	public class ExchangeResult
	{
		public decimal ExchangeRate { get; set; }
		public DateTime ExchangeDate { get; set; }
		public string Currency { get; set; } = default!;
	}
}
