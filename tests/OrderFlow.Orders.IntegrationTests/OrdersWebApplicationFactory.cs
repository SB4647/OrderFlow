using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using OrderFlow.Orders.Infrastructure.Persistence;

namespace OrderFlow.Orders.IntegrationTests;

public sealed class OrdersWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"orders-tests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureLogging(logging => logging.ClearProviders());
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<OrdersDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<OrdersDbContext>>();
            services.RemoveAll<OrdersDbContext>();
            services.AddDbContext<OrdersDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
        });
    }

}
