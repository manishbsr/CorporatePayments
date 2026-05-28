
using Microsoft.EntityFrameworkCore;
using PurchaseTransactions.Application.Interfaces;
using PurchaseTransactions.Infrastructure.ExternalServices;
using PurchaseTransactions.Infrastructure.Persistence;
using PurchaseTransactions.Infrastructure.Persistence.Repositiories;

namespace Microsoft.Extensions.DependencyInjection
{
	public static class IServiceCollectionExtensions
	{
		
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, string connectionString)
        {

            services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));
            services.AddScoped<IPurchaseTransactionRepository, PurchaseTransactionRepository>();
			services.AddSingleton<IExchangeRateService, ExchangeRateService>();
			services.AddHttpClient("ExchangeRateAPI", client =>
			{
				client.Timeout = TimeSpan.FromSeconds(10);
			});
			return services;
        }
    }
}
