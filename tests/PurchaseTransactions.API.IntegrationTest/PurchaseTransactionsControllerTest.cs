using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Newtonsoft.Json;
using PurchaseTransactions.Application.Interfaces;
using PurchaseTransactions.Application.Models;
using PurchaseTransactions.Domain.Models;

namespace PurchaseTransactions.API.IntegrationTest
{
	public class PurchaseTransactionsControllerTest : IClassFixture<UnitTestingWebApplicationFactory<Program>>
	{
		private readonly HttpClient _client;
		private readonly Mock<IExchangeRateService> _exchangeRateServiceMock;

		public PurchaseTransactionsControllerTest(UnitTestingWebApplicationFactory<Program> factory)
		{
			_exchangeRateServiceMock = new Mock<IExchangeRateService>();
			_client = factory.WithWebHostBuilder(builder =>
			{
				builder.ConfigureTestServices(services =>
				{
					services.RemoveAll(typeof(IExchangeRateService));
					services.AddScoped(_ => _exchangeRateServiceMock.Object);

				});
			})
			.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false }) ?? throw new ArgumentNullException(nameof(factory));

		}
		[Fact]
		public async Task Create_BadRequest_When_Command_Is_Null()
		{
			// Arrange
			// Act
			var response = await _client.PostAsJsonAsync("/api/PurchaseTransactions", default(CreatePurchaseTransactionRequest));
			var responseBody = await response.Content.ReadAsStringAsync();
			//Assert
			Assert.False(response.IsSuccessStatusCode, responseBody);
			Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
			Assert.Contains("Argument 'request' is missing", responseBody);
		}
		[Fact]
		public async Task Create_BadRequest_When_Description_Is_Null()
		{
			// Arrange
			var request = new CreatePurchaseTransactionRequest { Amount = 10.00M, TransactionDate = DateTime.UtcNow.AddMinutes(-1) };
			// Act
			var response = await _client.PostAsJsonAsync("/api/PurchaseTransactions", request);
			var responseBody = await response.Content.ReadAsStringAsync();
			//Assert
			Assert.False(response.IsSuccessStatusCode, responseBody);
			Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
			Assert.Contains("Description' must not be empty", responseBody);
		}
		[Fact]
		public async Task Create_BadRequest_When_Amount_Is_Zero()
		{
			// Arrange
			var request = new CreatePurchaseTransactionRequest { Amount = 0M, Description = "Test", TransactionDate = DateTime.UtcNow.AddMinutes(-1) };
			// Act
			var response = await _client.PostAsJsonAsync("/api/PurchaseTransactions", request);
			var responseBody = await response.Content.ReadAsStringAsync();
			//Assert
			Assert.False(response.IsSuccessStatusCode, responseBody);
			Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
			Assert.Contains("Purchase Amount must be greater than zero", responseBody);
		}

		[Fact]
		public async Task Create_BadRequest_When_Amount_Is_Negative()
		{
			// Arrange
			var request = new CreatePurchaseTransactionRequest { Amount = -50.00M, Description = "Test", TransactionDate = DateTime.UtcNow.AddMinutes(-1) };
			// Act
			var response = await _client.PostAsJsonAsync("/api/PurchaseTransactions", request);
			var responseBody = await response.Content.ReadAsStringAsync();
			//Assert
			Assert.False(response.IsSuccessStatusCode, responseBody);
			Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
			Assert.Contains("Purchase Amount must be greater than zero", responseBody);
		}

		[Fact]
		public async Task Create_BadRequest_When_Amount_Has_More_Than_Two_Decimals()
		{
			// Arrange
			var request = new CreatePurchaseTransactionRequest { Amount = 10.125M, Description = "Test", TransactionDate = DateTime.UtcNow.AddMinutes(-1) };
			// Act
			var response = await _client.PostAsJsonAsync("/api/PurchaseTransactions", request);
			var responseBody = await response.Content.ReadAsStringAsync();
			//Assert
			Assert.False(response.IsSuccessStatusCode, responseBody);
			Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
			Assert.Contains("Purchase Amount must have at most 2 decimal places", responseBody);
		}



		[Fact]
		public async Task Create_BadRequest_When_Description_Exceeds_MaxLength()
		{
			// Arrange
			var longDescription = new string('a', 51); // 51 characters, max is 50
			var request = new CreatePurchaseTransactionRequest { Amount = 10.00M, Description = longDescription, TransactionDate = DateTime.UtcNow.AddMinutes(-1) };
			// Act
			var response = await _client.PostAsJsonAsync("/api/PurchaseTransactions", request);
			var responseBody = await response.Content.ReadAsStringAsync();
			//Assert
			Assert.False(response.IsSuccessStatusCode, responseBody);
			Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
			Assert.Contains("Description must not exceed 50 characters", responseBody);
		}
		[Fact]
		public async Task Create_Returns_CreatedAtAction_With_Correct_StatusCode()
		{
			// Arrange
			var request = new CreatePurchaseTransactionRequest { Amount = 75.50M, Description = "Status Code Test", TransactionDate = DateTime.UtcNow.AddMinutes(-1) };
			// Act
			var response = await _client.PostAsJsonAsync("/api/PurchaseTransactions", request);
			//Assert
			Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
		}
		[Fact]
		public async Task Create_Success_With_Valid_Parameters()
		{
			// Arrange
			var request = new CreatePurchaseTransactionRequest
			{
				Amount = 250.50M,
				Description = "Office Supplies Purchase",
				TransactionDate = DateTime.UtcNow.AddDays(-5)
			};
			// Act
			var response = await _client.PostAsJsonAsync("/api/PurchaseTransactions", request);
			var responseBody = await response.Content.ReadAsStringAsync();
			//Assert
			Assert.True(response.IsSuccessStatusCode, responseBody);
			Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
			var transaction = JsonConvert.DeserializeObject<PurchaseTransaction>(responseBody)!;
			Assert.NotEmpty(transaction.TransactionId);
			Assert.Equal(transaction.AmountInUSD, request.Amount);
			Assert.Equal(transaction.Description, request.Description);
		}
		[Fact]
		public async Task Create_Success_Multiple_Transactions()
		{
			// Arrange & Act & Assert - Create first transaction
			var request1 = new CreatePurchaseTransactionRequest { Amount = 100.00M, Description = "Transaction 1", TransactionDate = DateTime.UtcNow.AddMinutes(-5) };
			var response1 = await _client.PostAsJsonAsync("/api/PurchaseTransactions", request1);
			Assert.True(response1.IsSuccessStatusCode);
			var transaction1 = JsonConvert.DeserializeObject<PurchaseTransaction>(await response1.Content.ReadAsStringAsync())!;

			// Arrange & Act & Assert - Create second transaction
			var request2 = new CreatePurchaseTransactionRequest { Amount = 200.00M, Description = "Transaction 2", TransactionDate = DateTime.UtcNow.AddMinutes(-10) };
			var response2 = await _client.PostAsJsonAsync("/api/PurchaseTransactions", request2);
			Assert.True(response2.IsSuccessStatusCode);
			var transaction2 = JsonConvert.DeserializeObject<PurchaseTransaction>(await response2.Content.ReadAsStringAsync())!;

			// Verify transactions are different
			Assert.NotEqual(transaction1.TransactionId, transaction2.TransactionId);
			Assert.Equal(transaction1.AmountInUSD, request1.Amount);
			Assert.Equal(transaction2.AmountInUSD, request2.Amount);
		}

		[Fact]
		public async Task GetTransaction_BadRequest_When_TransactionId_Is_Empty()
		{
			// Arrange
			// Act
			var response = await _client.GetAsync($"/api/PurchaseTransactions/?targetCurrency=EURO");
			//Assert
			Assert.False(response.IsSuccessStatusCode);
			Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
		}

		[Fact]
		public async Task GetTransaction_BadRequest_When_TargetCurrency_Is_Missing()
		{
			// Arrange
			var transactionId = Guid.NewGuid().ToString();
			// Act
			var response = await _client.GetAsync($"/api/PurchaseTransactions/{transactionId}");
			var responseBody = await response.Content.ReadAsStringAsync();
			//Assert
			Assert.False(response.IsSuccessStatusCode, responseBody);
			Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
		}
		[Fact]
		public async Task GetTransaction_With_Invalid_Guid_Format()
		{
			// Arrange
			var invalidId = "not-a-guid";
			// Act
			var response = await _client.GetAsync($"/api/PurchaseTransactions/{invalidId}?targetCurrency=EUR");
			//Assert
			Assert.False(response.IsSuccessStatusCode);
		}
		[Fact]
		public async Task GetTransaction_Success_With_Valid_Id_And_Currency()
		{
			// Arrange - Create a transaction first
			var createRequest = new CreatePurchaseTransactionRequest { Amount = 100.00M, Description = "Test Transaction", TransactionDate = DateTime.UtcNow.AddMinutes(-1) };
			var createResponse = await _client.PostAsJsonAsync("/api/PurchaseTransactions", createRequest);
			var createdTransaction = JsonConvert.DeserializeObject<PurchaseTransaction>(await createResponse.Content.ReadAsStringAsync())!;

			_exchangeRateServiceMock.Setup(t => t.GetExchangeRateAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(new ExchangeResult { Currency = "GBP", ExchangeRate = 0.73M, ExchangeDate = DateTime.UtcNow });

			// Act
			var response = await _client.GetAsync($"/api/PurchaseTransactions/{createdTransaction.TransactionId}?targetCurrency=GBP");
			var responseBody = await response.Content.ReadAsStringAsync();
			//Assert
			Assert.True(response.IsSuccessStatusCode, responseBody);
			var transaction = JsonConvert.DeserializeObject<PurchaseTransactionTargetCurrency>(responseBody)!;
			Assert.Equal(transaction.TransactionId, createdTransaction.TransactionId);
			Assert.Equal(transaction.AmountInUSD, createRequest.Amount);
			Assert.Equal("GBP", transaction.TargetCurrency);
			Assert.True(transaction.AmountWithTargetCurrent > 0);
		}

		[Fact]
		public async Task GetTransaction_Success_With_Different_Exchange_Rates()
		{
			// Arrange - Create a transaction
			var createRequest = new CreatePurchaseTransactionRequest { Amount = 500.00M, Description = "Exchange Rate Test", TransactionDate = DateTime.UtcNow.AddMinutes(-1) };
			var createResponse = await _client.PostAsJsonAsync("/api/PurchaseTransactions", createRequest);
			var createdTransaction = JsonConvert.DeserializeObject<PurchaseTransaction>(await createResponse.Content.ReadAsStringAsync())!;

			// Test with EUR exchange rate
			_exchangeRateServiceMock.Setup(t => t.GetExchangeRateAsync("EUR", It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(new ExchangeResult { Currency = "EUR", ExchangeRate = 0.92M, ExchangeDate = DateTime.UtcNow });

			var response1 = await _client.GetAsync($"/api/PurchaseTransactions/{createdTransaction.TransactionId}?targetCurrency=EUR");
			var transaction1 = JsonConvert.DeserializeObject<PurchaseTransactionTargetCurrency>(await response1.Content.ReadAsStringAsync())!;
			var eurAmount = transaction1.AmountWithTargetCurrent;

			// Test with JPY exchange rate
			_exchangeRateServiceMock.Setup(t => t.GetExchangeRateAsync("JPY", It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(new ExchangeResult { Currency = "JPY", ExchangeRate = 110.00M, ExchangeDate = DateTime.UtcNow });

			var response2 = await _client.GetAsync($"/api/PurchaseTransactions/{createdTransaction.TransactionId}?targetCurrency=JPY");
			var transaction2 = JsonConvert.DeserializeObject<PurchaseTransactionTargetCurrency>(await response2.Content.ReadAsStringAsync())!;
			var jpyAmount = transaction2.AmountWithTargetCurrent;

			// Assert
			Assert.True(response1.IsSuccessStatusCode);
			Assert.True(response2.IsSuccessStatusCode);
			Assert.NotEqual(eurAmount, jpyAmount);
			Assert.True(jpyAmount > eurAmount); // JPY is higher value
		}

		[Fact]
		public async Task Search_BadRequest_When_Currency_Is_Empty()
		{
			// Arrange
			// Act
			var response = await _client.GetAsync("/api/PurchaseTransactions?currency=");
			var responseBody = await response.Content.ReadAsStringAsync();
			//Assert
			Assert.False(response.IsSuccessStatusCode, responseBody);
			Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
		}

		[Fact]
		public async Task Search_Success_Returns_Empty_List_When_No_Transactions()
		{
			// Arrange
			// Act
			var response = await _client.GetAsync("/api/PurchaseTransactions?currency=EURO");
			var responseBody = await response.Content.ReadAsStringAsync();
			//Assert
			Assert.True(response.IsSuccessStatusCode, responseBody);
			var transactions = JsonConvert.DeserializeObject<List<PurchaseTransactionTargetCurrency>>(responseBody)!;
			Assert.Empty(transactions);
		}

		[Fact]
		public async Task Search_Success_Returns_Transactions_With_Filters()
		{
			// Arrange - Create multiple transactions
			var request1 = new CreatePurchaseTransactionRequest { Amount = 100.00M, Description = "Small Purchase", TransactionDate = DateTime.UtcNow.AddDays(-10) };
			await _client.PostAsJsonAsync("/api/PurchaseTransactions", request1);

			var request2 = new CreatePurchaseTransactionRequest { Amount = 500.00M, Description = "Large Purchase", TransactionDate = DateTime.UtcNow.AddDays(-5) };
			await _client.PostAsJsonAsync("/api/PurchaseTransactions", request2);

			var request3 = new CreatePurchaseTransactionRequest { Amount = 250.00M, Description = "Medium Purchase", TransactionDate = DateTime.UtcNow.AddDays(-1) };
			await _client.PostAsJsonAsync("/api/PurchaseTransactions", request3);

			// Act - Search with various filters
			var response = await _client.GetAsync("/api/PurchaseTransactions?currency=USD&minAmount=150&maxAmount=600");
			var responseBody = await response.Content.ReadAsStringAsync();
			//Assert
			Assert.True(response.IsSuccessStatusCode, responseBody);
			var transactions = JsonConvert.DeserializeObject<List<PurchaseTransactionTargetCurrency>>(responseBody)!;
			Assert.NotEmpty(transactions);
			// Should contain transactions in the amount range
			Assert.All(transactions, t => Assert.True(t.AmountInUSD >= 150 && t.AmountInUSD <= 600));
		}

		[Fact]
		public async Task Search_Success_With_Date_Range_Filter()
		{
			// Arrange - Create transactions with different dates
			var startDate = DateTime.UtcNow.AddDays(-10);
			var endDate = DateTime.UtcNow.AddDays(-5);

			var request1 = new CreatePurchaseTransactionRequest { Amount = 100.00M, Description = "Older Transaction", TransactionDate = startDate.AddDays(-1) };
			await _client.PostAsJsonAsync("/api/PurchaseTransactions", request1);

			var request2 = new CreatePurchaseTransactionRequest { Amount = 200.00M, Description = "In Range Transaction", TransactionDate = endDate.AddDays(-2) };
			await _client.PostAsJsonAsync("/api/PurchaseTransactions", request2);

			var request3 = new CreatePurchaseTransactionRequest { Amount = 300.00M, Description = "Recent Transaction", TransactionDate = DateTime.UtcNow.AddMinutes(-1) };
			await _client.PostAsJsonAsync("/api/PurchaseTransactions", request3);

			// Act - Search with date range
			var response = await _client.GetAsync($"/api/PurchaseTransactions?currency=USD&startDate={startDate:O}&endDate={endDate:O}");
			var responseBody = await response.Content.ReadAsStringAsync();
			//Assert
			Assert.True(response.IsSuccessStatusCode, responseBody);
			var transactions = JsonConvert.DeserializeObject<List<PurchaseTransactionTargetCurrency>>(responseBody)!;
			Assert.NotEmpty(transactions);
		}
		[Fact]
		public async Task Create_Transaction_Then_Verify_It_Can_Be_Retrieved()
		{
			// Arrange
			var request = new CreatePurchaseTransactionRequest { Amount = 150.75M, Description = "Verification Test", TransactionDate = DateTime.UtcNow.AddMinutes(-1) };

			// Act - Create transaction
			var createResponse = await _client.PostAsJsonAsync("/api/PurchaseTransactions", request);
			var createdTransaction = JsonConvert.DeserializeObject<PurchaseTransaction>(await createResponse.Content.ReadAsStringAsync())!;

			_exchangeRateServiceMock.Setup(t => t.GetExchangeRateAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(new ExchangeResult { Currency = "USD", ExchangeRate = 1.00M, ExchangeDate = DateTime.UtcNow });

			// Act - Retrieve transaction
			var getResponse = await _client.GetAsync($"/api/PurchaseTransactions/{createdTransaction.TransactionId}?targetCurrency=USD");
			var retrievedTransaction = JsonConvert.DeserializeObject<PurchaseTransactionTargetCurrency>(await getResponse.Content.ReadAsStringAsync())!;

			// Assert
			Assert.Equal(createdTransaction.TransactionId, retrievedTransaction.TransactionId);
			Assert.Equal(createdTransaction.Description, retrievedTransaction.Description);
			Assert.Equal(createdTransaction.AmountInUSD, retrievedTransaction.AmountInUSD);
		}
	}
}
