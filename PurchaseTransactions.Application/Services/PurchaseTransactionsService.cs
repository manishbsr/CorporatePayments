using FluentValidation;
using Microsoft.Extensions.Logging;
using PurchaseTransactions.Application.Exceptions;
using PurchaseTransactions.Application.Interfaces;
using PurchaseTransactions.Application.Models;
using PurchaseTransactions.Domain.Models;

namespace PurchaseTransactions.Application.Services
{
	public class PurchaseTransactionsService : IPurchaseTransactionsService
	{
		private readonly IValidator<CreatePurchaseTransactionRequest> _createValidator;
		private readonly IValidator<SearchPurchaseTransactionsRequest> _searchValidator;
		private readonly IValidator<GetPurchaseTransactionRequest> _getValidator;
		private readonly ILogger<PurchaseTransactionsService> _logger;
		private readonly IPurchaseTransactionRepository _purchaseTransactionRepository;
		private readonly IExchangeRateService _exchangeRateService;

		public PurchaseTransactionsService(
			IValidator<CreatePurchaseTransactionRequest> createValidator,
			IValidator<SearchPurchaseTransactionsRequest> searchValidator,
			IValidator<GetPurchaseTransactionRequest> getValidator,
			IPurchaseTransactionRepository purchaseTransactionRepository,
			IExchangeRateService exchangeRateService,
			ILogger<PurchaseTransactionsService> logger)
		{
			_createValidator = createValidator ?? throw new ArgumentNullException(nameof(createValidator));
			_searchValidator = searchValidator ?? throw new ArgumentNullException(nameof(searchValidator));
			_getValidator = getValidator ?? throw new ArgumentNullException(nameof(getValidator));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_purchaseTransactionRepository = purchaseTransactionRepository ?? throw new ArgumentNullException(nameof(purchaseTransactionRepository));
			_exchangeRateService = exchangeRateService ?? throw new ArgumentNullException(nameof(exchangeRateService));
		}

		public async Task<PurchaseTransaction> CreateAsync(CreatePurchaseTransactionRequest request, CancellationToken cancellationToken = default)
		{
			_logger.LogDebug("CreateAsync Started");
			if (request is null)
			{
				throw new MissingArgumentException(nameof(request));
			}

			var validation = await _createValidator
				.ValidateAsync(request, cancellationToken)
				.ConfigureAwait(continueOnCapturedContext: false);
			if (!validation.IsValid)
			{
				throw new Exceptions.ValidationException(validation.Errors);
			}
			return await _purchaseTransactionRepository.AddAsync(request, cancellationToken);
		}

		public async Task<List<PurchaseTransactionTargetCurrency>> SearchAsync(SearchPurchaseTransactionsRequest request, CancellationToken cancellationToken = default)
		{
			_logger.LogDebug("SearchAsync Started");
			if (request is null)
			{
				throw new MissingArgumentException(nameof(request));
			}

			var validation = await _searchValidator
				.ValidateAsync(request, cancellationToken)
				.ConfigureAwait(continueOnCapturedContext: false);
			if (!validation.IsValid)
			{
				throw new Exceptions.ValidationException(validation.Errors);
			}


			var purchaseTransactions = await _purchaseTransactionRepository.SearchAsync(request, cancellationToken);
			var result = new List<PurchaseTransactionTargetCurrency>();

			foreach (var purchaseTransaction in purchaseTransactions)
			{
				var transaction = new PurchaseTransactionTargetCurrency
				{
					TransactionId = purchaseTransaction.TransactionId,
					AmountInUSD = purchaseTransaction.AmountInUSD,
					TransactionDate = purchaseTransaction.TransactionDate,
					Description = purchaseTransaction.Description,
					TargetCurrency = request.Currency
				};

				try
				{
					var exchangeResult = await _exchangeRateService.GetExchangeRateAsync(request.Currency, purchaseTransaction.TransactionDate, cancellationToken);
					if (exchangeResult != null)
					{
						transaction.AmountWithTargetCurrent = Math.Round(purchaseTransaction.AmountInUSD * exchangeResult.ExchangeRate, 2);
					}
					else
					{
						transaction.Error = new Error()
						{
							Code = System.Net.HttpStatusCode.NotFound,
							Message = $"No exchange rate found for {request.Currency}  within 6 months of {purchaseTransaction.TransactionDate:yyyy-MM-dd}"
						};
					}
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Error retrieving exchange rate for currency:{Currency} on date:{Date}", request.Currency, purchaseTransaction.TransactionDate);
					transaction.Error = new Error()
					{
						Code = System.Net.HttpStatusCode.ServiceUnavailable,
						Message = $"failed to reterive exchange rate for currency:{request.Currency}"
					};
				}

				result.Add(transaction);
			}

			return result;
		}

		public async Task<PurchaseTransactionTargetCurrency> GetTransactionAsync(GetPurchaseTransactionRequest request, CancellationToken cancellationToken = default)
		{
			_logger.LogDebug("GetTransactionAsync Started");
			if (request is null)
			{
				throw new MissingArgumentException(nameof(request));
			}

			var validation = await _getValidator
				.ValidateAsync(request, cancellationToken)
				.ConfigureAwait(continueOnCapturedContext: false);
			if (!validation.IsValid)
			{
				throw new Exceptions.ValidationException(validation.Errors);
			}

			
			var purchaseTransaction = await _purchaseTransactionRepository.GetPurchaseTransactionsAsync(request, cancellationToken)!;
			var transaction = new PurchaseTransactionTargetCurrency
			{
				TransactionId = purchaseTransaction.TransactionId,
				AmountInUSD = purchaseTransaction.AmountInUSD,
				TransactionDate = purchaseTransaction.TransactionDate,
				Description = purchaseTransaction.Description,
				TargetCurrency = request.Currency
			};

			var exchangeResult = await _exchangeRateService.GetExchangeRateAsync(request.Currency, purchaseTransaction.TransactionDate, cancellationToken) ?? throw new NotFoundException($"No exchange rate found for {request.Currency}  within 6 months of {purchaseTransaction.TransactionDate:yyyy-MM-dd}");
			transaction.AmountWithTargetCurrent = Math.Round(purchaseTransaction.AmountInUSD * exchangeResult.ExchangeRate, 2);
			return transaction;
		}
	}
}
