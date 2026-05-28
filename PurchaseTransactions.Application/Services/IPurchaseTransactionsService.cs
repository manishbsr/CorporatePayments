using PurchaseTransactions.Application.Models;
using PurchaseTransactions.Domain.Models;

namespace PurchaseTransactions.Application.Services
{
	public interface IPurchaseTransactionsService
	{
		public Task<PurchaseTransaction> CreateAsync(CreatePurchaseTransactionRequest request, CancellationToken cancellationToken = default);
		public Task<List<PurchaseTransactionTargetCurrency>> SearchAsync(SearchPurchaseTransactionsRequest request, CancellationToken cancellationToken = default);
		public Task<PurchaseTransactionTargetCurrency> GetTransactionAsync(GetPurchaseTransactionRequest request, CancellationToken cancellationToken = default);
	}
}
