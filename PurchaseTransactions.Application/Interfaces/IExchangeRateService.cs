using PurchaseTransactions.Domain.Models;

namespace PurchaseTransactions.Application.Interfaces
{
	public interface IExchangeRateService
	{
		public Task<ExchangeResult?> GetExchangeRateAsync(string currency, DateTime date, CancellationToken cancellationToken = default);
	}
}
