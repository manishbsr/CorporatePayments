using Microsoft.Extensions.Logging;
using Moq;
using PurchaseTransactions.Application.Exceptions;
using PurchaseTransactions.Application.Interfaces;
using PurchaseTransactions.Application.Models;
using PurchaseTransactions.Application.Services;
using PurchaseTransactions.Domain.Models;

namespace PurchaseTransctions.UnitTest.Application
{
	public class PurchaseTransactionsServiceTest
	{

		private readonly Mock<IPurchaseTransactionRepository> _purchaseTransactionRepositoryMock;
		private readonly Mock<IExchangeRateService> _exchangeRateServiceMock;
		private readonly Mock<ILogger<PurchaseTransactionsService>> _loggerMock;
		private readonly PurchaseTransactionsService _service;

		public PurchaseTransactionsServiceTest()
		{

			_purchaseTransactionRepositoryMock = new Mock<IPurchaseTransactionRepository>();
			_exchangeRateServiceMock = new Mock<IExchangeRateService>();
			_loggerMock = new Mock<ILogger<PurchaseTransactionsService>>();

			_service = new PurchaseTransactionsService(
				new CreatePurchaseTransactionRequestValidator(),
				new SearchPurchaseTransactionsRequestValidator(),
				new GetPurchaseTransactionRequestValidator(),
				_purchaseTransactionRepositoryMock.Object,
				_exchangeRateServiceMock.Object,
				_loggerMock.Object);
		}

		#region CreateAsync Tests

		[Fact]
		public async Task CreateAsync_NullRequest_ThrowsMissingArgumentException()
		{
			await Assert.ThrowsAsync<MissingArgumentException>(() => _service.CreateAsync(null!, CancellationToken.None));
		}

		[Fact]
		public async Task CreateAsync_ValidationFails_ThrowsValidationException()
		{
			var request = new CreatePurchaseTransactionRequest();
			await Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(request, CancellationToken.None));
		}

		[Fact]
		public async Task CreateAsync_RepositoryThrows_ExceptionPropagates()
		{
			var request = new CreatePurchaseTransactionRequest { Description = "desc", TransactionDate = DateTime.UtcNow.AddMinutes(-1), Amount = 5.00M };
			

			_purchaseTransactionRepositoryMock
				.Setup(r => r.AddAsync(It.IsAny<CreatePurchaseTransactionRequest>(), It.IsAny<CancellationToken>()))
				.ThrowsAsync(new InvalidOperationException("db error"));

			await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(request, CancellationToken.None));
		}

		[Fact]
		public async Task CreateAsync_Success_ReturnsPurchaseTransaction()
		{
			var description = "Test Purchase";
			var transactionDate = DateTime.UtcNow.AddMinutes(-5);
			var amount = 25.50M;
			var request = new CreatePurchaseTransactionRequest { Description = description, TransactionDate = transactionDate, Amount = amount };
			
			var expected = new PurchaseTransaction { TransactionId = "tx-1", Description = description, TransactionDate = transactionDate, AmountInUSD = amount };

			

			_purchaseTransactionRepositoryMock
				.Setup(r => r.AddAsync(It.IsAny<CreatePurchaseTransactionRequest>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(expected);

			var result = await _service.CreateAsync(request, CancellationToken.None);

			Assert.NotNull(result);
			Assert.Equal(expected.TransactionId, result.TransactionId);
			Assert.Equal(expected.Description, result.Description);
			Assert.Equal(expected.AmountInUSD, result.AmountInUSD);
		}

		#endregion

		#region SearchAsync Tests

		[Fact]
		public async Task SearchAsync_NullRequest_ThrowsMissingArgumentException()
		{
			await Assert.ThrowsAsync<MissingArgumentException>(() => _service.SearchAsync(null!, CancellationToken.None));
		}

		[Fact]
		public async Task SearchAsync_ValidationFails_NoCurrency_ThrowsValidationException()
		{
			var request = new SearchPurchaseTransactionsRequest();
			

			await Assert.ThrowsAsync<ValidationException>(() => _service.SearchAsync(request, CancellationToken.None));
		}

		[Fact]
		public async Task SearchAsync_Success_ExchangeRateNull_ReturnsResultWithError()
		{
			var request = new SearchPurchaseTransactionsRequest { Currency = "EUR" };
			
			var purchaseList = new List<PurchaseTransaction>
			{
				new PurchaseTransaction { TransactionId = "1", Description = "a", TransactionDate = DateTime.UtcNow.AddDays(-10), AmountInUSD = 10M },
				new PurchaseTransaction { TransactionId = "2", Description = "b", TransactionDate = DateTime.UtcNow.AddDays(-5), AmountInUSD = 20M }
			};


			_purchaseTransactionRepositoryMock
				.Setup(r => r.SearchAsync(It.IsAny<SearchPurchaseTransactionsRequest>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(purchaseList);

			_exchangeRateServiceMock
				.Setup(s => s.GetExchangeRateAsync("EUR", It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync((ExchangeResult?)null);

			var result = await _service.SearchAsync(request, CancellationToken.None);

			Assert.NotNull(result);
			Assert.Equal(2, result.Count);
			Assert.All(result, r => Assert.NotNull(r.Error));
		}

		[Fact]
		public async Task SearchAsync_ExchangeServiceThrows_ErrorIsSetOnResult()
		{
			var request = new SearchPurchaseTransactionsRequest { Currency = "EUR" };
			
			var purchaseList = new List<PurchaseTransaction>
			{
				new PurchaseTransaction { TransactionId = "1", Description = "a", TransactionDate = DateTime.UtcNow.AddDays(-10), AmountInUSD = 10M }
			};

		
			_purchaseTransactionRepositoryMock
				.Setup(r => r.SearchAsync(It.IsAny<SearchPurchaseTransactionsRequest>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(purchaseList);

			_exchangeRateServiceMock
				.Setup(s => s.GetExchangeRateAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
				.ThrowsAsync(new InvalidOperationException("svc"));

			var result = await _service.SearchAsync(request, CancellationToken.None);

			Assert.Single(result);
			Assert.NotNull(result[0].Error);
			Assert.Equal(System.Net.HttpStatusCode.ServiceUnavailable, result[0].Error.Code);
		}

		[Fact]
		public async Task SearchAsync_RepositoryThrows_ExceptionPropagates()
		{
			var request = new SearchPurchaseTransactionsRequest { Currency = "EUR" };
			
			_purchaseTransactionRepositoryMock
				.Setup(r => r.SearchAsync(It.IsAny<SearchPurchaseTransactionsRequest>(), It.IsAny<CancellationToken>()))
				.ThrowsAsync(new InvalidOperationException("db"));

			await Assert.ThrowsAsync<InvalidOperationException>(() => _service.SearchAsync(request, CancellationToken.None));
		}

		#endregion

		#region GetTransactionAsync Tests

		[Fact]
		public async Task GetTransactionAsync_NullRequest_ThrowsMissingArgumentException()
		{
			await Assert.ThrowsAsync<MissingArgumentException>(() => _service.GetTransactionAsync(null!, CancellationToken.None));
		}

		[Fact]
		public async Task GetTransactionAsync_ValidationFails_ThrowsValidationException()
		{
			var request = new GetPurchaseTransactionRequest();
		

			await Assert.ThrowsAsync<ValidationException>(() => _service.GetTransactionAsync(request, CancellationToken.None));
		}

		[Fact]
		public async Task GetTransactionAsync_Success_ReturnsTargetCurrencyModel()
		{
			var transactionId = Guid.NewGuid().ToString();
			var request = new GetPurchaseTransactionRequest { Currency = "EUR", TransactionId = transactionId };
	
			var purchase = new PurchaseTransaction { TransactionId = transactionId, Description = "d", TransactionDate = DateTime.UtcNow.AddDays(-1), AmountInUSD = 100M };

		

			_purchaseTransactionRepositoryMock
				.Setup(r => r.GetPurchaseTransactionsAsync(It.IsAny<GetPurchaseTransactionRequest>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(purchase);

			_exchangeRateServiceMock
				.Setup(s => s.GetExchangeRateAsync("EUR", purchase.TransactionDate, It.IsAny<CancellationToken>()))
				.ReturnsAsync(new ExchangeResult { ExchangeRate = 0.85M, ExchangeDate = purchase.TransactionDate, Currency = "EUR" });

			var result = await _service.GetTransactionAsync(request, CancellationToken.None);

			Assert.NotNull(result);
			Assert.Equal(purchase.TransactionId, result.TransactionId);
			Assert.Equal("EUR", result.TargetCurrency);
			Assert.Equal(Math.Round(purchase.AmountInUSD * 0.85M, 2), result.AmountWithTargetCurrent);
		}

		[Fact]
		public async Task GetTransactionAsync_ExchangeRateNotFound_ThrowsNotFoundException()
		{
			var transactionId = Guid.NewGuid().ToString();
			var request = new GetPurchaseTransactionRequest { Currency = "EUR", TransactionId = transactionId };
			
			var purchase = new PurchaseTransaction { TransactionId = transactionId, Description = "d", TransactionDate = DateTime.UtcNow.AddDays(-1), AmountInUSD = 100M };

		

			_purchaseTransactionRepositoryMock
				.Setup(r => r.GetPurchaseTransactionsAsync(It.IsAny<GetPurchaseTransactionRequest>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(purchase);

			_exchangeRateServiceMock
				.Setup(s => s.GetExchangeRateAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync((ExchangeResult?)null);

			await Assert.ThrowsAsync<NotFoundException>(() => _service.GetTransactionAsync(request, CancellationToken.None));
		}

		[Fact]
		public async Task GetTransactionAsync_ExchangeServiceThrows_ExceptionPropagates()
		{
			var transactionId = Guid.NewGuid().ToString();
			var request = new GetPurchaseTransactionRequest { Currency = "EUR", TransactionId = transactionId };
			
			var purchase = new PurchaseTransaction { TransactionId = transactionId, Description = "d", TransactionDate = DateTime.UtcNow.AddDays(-1), AmountInUSD = 100M };


			_purchaseTransactionRepositoryMock
				.Setup(r => r.GetPurchaseTransactionsAsync(It.IsAny<GetPurchaseTransactionRequest>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(purchase);

			_exchangeRateServiceMock
				.Setup(s => s.GetExchangeRateAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
				.ThrowsAsync(new InvalidOperationException("svc"));

			await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetTransactionAsync(request, CancellationToken.None));
		}

		[Fact]
		public async Task GetTransactionAsync_RepositoryThrows_ExceptionPropagates()
		{
			var transactionId = Guid.NewGuid().ToString();
			var request = new GetPurchaseTransactionRequest { Currency = "EUR", TransactionId = transactionId };
		

			_purchaseTransactionRepositoryMock
				.Setup(r => r.GetPurchaseTransactionsAsync(It.IsAny<GetPurchaseTransactionRequest>(), It.IsAny<CancellationToken>()))
				.ThrowsAsync(new NotFoundException("not found"));

			await Assert.ThrowsAsync<NotFoundException>(() => _service.GetTransactionAsync(request, CancellationToken.None));
		}

		#endregion
	}
}
