using PurchaseTransactions.Application.Models;
using PurchaseTransactions.Domain.Models;

namespace PurchaseTransactions.Application.Interfaces
{
	public interface IPurchaseTransactionRepository
	{
		public Task<PurchaseTransaction> AddAsync(CreatePurchaseTransactionRequest createPurchaseTransactionRequest, CancellationToken cancellationToken = default);
		public Task<List<PurchaseTransaction>> SearchAsync(SearchPurchaseTransactionsRequest searchPurchaseTransactionsRequest, CancellationToken cancellationToken = default);
		public Task<PurchaseTransaction> GetPurchaseTransactionsAsync(GetPurchaseTransactionRequest getPurchaseTransactionRequest, CancellationToken cancellationToken = default);
	}
}
