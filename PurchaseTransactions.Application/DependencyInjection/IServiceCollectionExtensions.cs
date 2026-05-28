
using FluentValidation;
using PurchaseTransactions.Application.Models;
using PurchaseTransactions.Application.Services;

namespace Microsoft.Extensions.DependencyInjection
{
	public static class IServiceCollectionExtensions
	{

		public static IServiceCollection AddApplicationServices(this IServiceCollection services)
		{

			services
				.AddScoped<IPurchaseTransactionsService, PurchaseTransactionsService>();
			services
				.AddScoped<IValidator<CreatePurchaseTransactionRequest>, CreatePurchaseTransactionRequestValidator>();
			services
				.AddScoped<IValidator<SearchPurchaseTransactionsRequest>, SearchPurchaseTransactionsRequestValidator>();
			services
				.AddScoped<IValidator<GetPurchaseTransactionRequest>, GetPurchaseTransactionRequestValidator>();

			return services;
		}
	}
}
