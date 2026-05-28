using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PurchaseTransactions.Infrastructure.Persistence;

namespace PurchaseTransactions.API.IntegrationTest
{
	public sealed class UnitTestingWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup>
		where TStartup : class

	{
		
		protected override void ConfigureWebHost(IWebHostBuilder builder)
		{
			
			builder.ConfigureServices(services =>
			{
				// In tests we want controller actions to run even when the body is null so tests
				// can verify controller-level handling instead of automatic 400 from ApiController.
				services.Configure<ApiBehaviorOptions>(options =>
				{
					options.SuppressModelStateInvalidFilter = true;
				});
				var descriptor = services.SingleOrDefault(
				   d => d.ServiceType ==
					   typeof(DbContextOptions<AppDbContext>));
				if (descriptor != null)
				{
					services.Remove(descriptor);
				}


				var serviceProvider = new ServiceCollection()
				.AddEntityFrameworkInMemoryDatabase()
				.BuildServiceProvider();
				var databaseName = Guid.NewGuid().ToString();

				services.AddDbContext<AppDbContext>(options =>
				{

					options.UseInMemoryDatabase(databaseName);
					options.UseInternalServiceProvider(serviceProvider);
				});
				
				
				// Build the service provider.
				var sp = services.BuildServiceProvider();

				// Create a scope to obtain a reference to the database contexts
				using var scope = sp.CreateScope();

				

			});
			
			base.ConfigureWebHost(builder);
		}
	}
}
