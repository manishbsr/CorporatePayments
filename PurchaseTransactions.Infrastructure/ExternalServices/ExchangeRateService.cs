using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using PurchaseTransactions.Application.Interfaces;
using PurchaseTransactions.Domain.Models;

namespace PurchaseTransactions.Infrastructure.ExternalServices
{
	public class ExchangeRateService : IExchangeRateService
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly string _baseUrl;

		public ExchangeRateService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
		{
			_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
			_baseUrl = configuration["ExchangeRateApi:BaseUrl"] ?? throw new InvalidOperationException("Exchange rate API base URL is not configured.");
		}
		public async Task<ExchangeResult?> GetExchangeRateAsync(string currency, DateTime date, CancellationToken cancellationToken = default)
		{
			var startDate = date.AddMonths(-6);
			var filter = $"filter=currency:eq:{currency},record_date:gte:{startDate:yyyy-MM-dd},record_date:lte:{date:yyyy-MM-dd}";
			var requestUrl = $"{_baseUrl}?fields=exchange_rate,record_date,currency&{filter}&sort=-record_date";
			using var httpClient = _httpClientFactory.CreateClient("ExchangeRateAPI");
			var response = await httpClient.GetAsync(requestUrl, cancellationToken);
			response.EnsureSuccessStatusCode();
			var result = await response.Content.ReadFromJsonAsync<ExchangeRateApiResponse>(cancellationToken: cancellationToken);
			if (result?.ApiRecords == null || result.ApiRecords.Count == 0)
			{
				return null;
			}
			var rate = result.ApiRecords[0];

			var exchangeResult = new ExchangeResult
			{
				Currency = rate.Currency,
				ExchangeRate = decimal.Parse(rate.ExchangeRate),
				ExchangeDate = DateTime.Parse(rate.RecordDate)
			};
			return exchangeResult;
		}
	}
	internal class  ExchangeRateApiResponse
	{
		[JsonPropertyName("data")]
		public List<ExchangeRateApiRecord> ApiRecords { get; set; } = new();
	}
	internal class ExchangeRateApiRecord
	{
		[JsonPropertyName("currency")]
		public string Currency { get; set; } = default!;
		[JsonPropertyName("exchange_rate")]
		public string ExchangeRate { get; set; } = default!;
		[JsonPropertyName("record_date")]
		public string RecordDate { get; set; } = default!;
	}
}
