namespace PurchaseTransactions.Domain.Models
{
    public class PurchaseTransaction
    {
        public string TransactionId { get; set; } = default!;
        public string Description { get; set; } = default!;
        public DateTime TransactionDate { get; set; } 
        public decimal AmountInUSD { get; set; }
    
    }
}
