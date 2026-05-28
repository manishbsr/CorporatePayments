namespace PurchaseTransactions.Infrastructure.Persistence.Entities
{
    public class PurchaseTransaction
    {
        public Guid TransactionId { get; set; } = Guid.NewGuid();
		public string Description { get; set; } = default!;
        public DateTime TransactionDate { get; set; } 
        public decimal Amount { get; set; }
    
    }
}
