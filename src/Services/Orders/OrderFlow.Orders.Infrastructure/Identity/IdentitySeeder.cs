using Microsoft.AspNetCore.Identity;

namespace OrderFlow.Orders.Infrastructure.Identity;

public sealed class IdentitySeeder(RoleManager<ApplicationRole> roleManager)
{
    public async Task SeedRolesAsync(CancellationToken cancellationToken)
    {
        foreach (var roleName in new[] { Roles.Customer, Roles.Admin })
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(new ApplicationRole(roleName));
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException($"Unable to create the '{roleName}' role.");
                }
            }
        }
    }
}
