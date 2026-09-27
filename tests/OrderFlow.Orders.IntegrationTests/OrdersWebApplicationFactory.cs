using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using OrderFlow.Orders.Application;
using OrderFlow.Orders.Infrastructure.Identity;
using OrderFlow.Orders.Infrastructure.Persistence;

namespace OrderFlow.Orders.IntegrationTests;

public sealed class OrdersWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"orders-tests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("Jwt:Issuer", "OrderFlow.Tests");
        builder.UseSetting("Jwt:Audience", "OrderFlow.Tests");
        builder.UseSetting("Jwt:Key", "orderflow-test-signing-key-with-at-least-32-characters");
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

    public async Task<AuthenticationResponse> CreateAdminAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<ApplicationUser>>();
        var tokenGenerator = scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>();
        var email = $"admin-{Guid.NewGuid():N}@example.test";
        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = email, Email = email };

        var result = await userManager.CreateAsync(user, "AdminPass1");
        if (!result.Succeeded)
        {
            throw new InvalidOperationException("Unable to create test administrator.");
        }

        await userManager.AddToRoleAsync(user, Roles.Admin);
        return tokenGenerator.CreateToken(new CurrentUser(user.Id, user.Email, Roles.Admin));
    }

}
