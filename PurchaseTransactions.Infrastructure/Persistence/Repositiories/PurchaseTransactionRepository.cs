using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PurchaseTransactions.Application.Exceptions;
using PurchaseTransactions.Application.Interfaces;
using PurchaseTransactions.Application.Models;
using PurchaseTransactions.Domain.Models;
namespace PurchaseTransactions.Infrastructure.Persistence.Repositiories
{
    public class PurchaseTransactionRepository : IPurchaseTransactionRepository
    {
        private readonly ILogger<PurchaseTransactionRepository> _logger;
        private readonly AppDbContext _appDbContext;
        public PurchaseTransactionRepository(ILogger<PurchaseTransactionRepository> logger, AppDbContext appDbContext)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _appDbContext = appDbContext ?? throw new ArgumentNullException(nameof(appDbContext));
        }
        public async Task<PurchaseTransaction> AddAsync(CreatePurchaseTransactionRequest createPurchaseTransactionRequest, CancellationToken cancellationToken = default)
        {
            if (createPurchaseTransactionRequest == null)
            {
                throw new MissingArgumentException(nameof(createPurchaseTransactionRequest));
            }
            var entity = ToEnity(createPurchaseTransactionRequest);
            await _appDbContext.PurchaseTransactions.AddAsync(entity, cancellationToken);
            await _appDbContext.SaveChangesAsync(cancellationToken);
            return ToDomainModel(entity);
        }
		public async Task<List<PurchaseTransaction>> SearchAsync(SearchPurchaseTransactionsRequest searchPurchaseTransactionsRequest , CancellationToken cancellationToken = default)
		{
			if (searchPurchaseTransactionsRequest == null)
			{
				throw new MissingArgumentException(nameof(searchPurchaseTransactionsRequest));
			}

			var dbQuery = _appDbContext.PurchaseTransactions.AsQueryable();
			if(searchPurchaseTransactionsRequest.StartDate.HasValue)
			{
				dbQuery = dbQuery.Where(x => x.TransactionDate >= searchPurchaseTransactionsRequest.StartDate.Value);
			}
			if(searchPurchaseTransactionsRequest.EndDate.HasValue)
			{
				dbQuery = dbQuery.Where(x => x.TransactionDate <= searchPurchaseTransactionsRequest.EndDate.Value);
			}
			if(searchPurchaseTransactionsRequest.MinAmount.HasValue)
			{
				dbQuery = dbQuery.Where(x => x.Amount >= searchPurchaseTransactionsRequest.MinAmount.Value);
			}
			if(searchPurchaseTransactionsRequest.MaxAmount.HasValue)
			{
				dbQuery = dbQuery.Where(x => x.Amount <= searchPurchaseTransactionsRequest.MaxAmount.Value);
			}
			var entities = await dbQuery.OrderByDescending(t => t.TransactionDate).ToListAsync(cancellationToken);
			return entities.Select(e => ToDomainModel(e)).ToList();

		}
		public async Task<PurchaseTransaction> GetPurchaseTransactionsAsync(GetPurchaseTransactionRequest getPurchaseTransactionRequest, CancellationToken cancellationToken = default)
		{

			if (getPurchaseTransactionRequest == null)
			{
				throw new MissingArgumentException(nameof(getPurchaseTransactionRequest));
			}

			if (!Guid.TryParse(getPurchaseTransactionRequest.TransactionId, out var transactionGuid))
			{
				throw new ValidationException("TransactionId is not in a valid format.");
			}

			var entity = await _appDbContext.PurchaseTransactions.FirstOrDefaultAsync(t => t.TransactionId == transactionGuid, cancellationToken) ?? throw new NotFoundException($"Purchase transaction with id {getPurchaseTransactionRequest.TransactionId} not found.");
			return await Task.FromResult(ToDomainModel(entity));
		}
		private Entities.PurchaseTransaction ToEnity(CreatePurchaseTransactionRequest request)
        {
            return new Entities.PurchaseTransaction
            {

                Description = request.Description,
                TransactionDate = request.TransactionDate,
                Amount = request.Amount
            };
        }
        private Domain.Models.PurchaseTransaction ToDomainModel(Entities.PurchaseTransaction purchaseTransaction)
        {
            return new Domain.Models.PurchaseTransaction
            {
                TransactionId = purchaseTransaction.TransactionId.ToString(),
                Description = purchaseTransaction.Description,
                TransactionDate = purchaseTransaction.TransactionDate,
                AmountInUSD = purchaseTransaction.Amount
            };
        }

		
	}
}
