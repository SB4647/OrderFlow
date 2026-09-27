using Microsoft.AspNetCore.Identity;
using OrderFlow.Orders.Application;

namespace OrderFlow.Orders.Infrastructure.Identity;

public sealed class IdentityAuthenticationService(
    UserManager<ApplicationUser> userManager,
    IJwtTokenGenerator tokenGenerator) : IAuthenticationService
{
    public async Task<AuthenticationResponse?> RegisterAsync(
        RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        var email = command.Email.Trim().ToLowerInvariant();
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email
        };
        var result = await userManager.CreateAsync(user, command.Password);
        if (!result.Succeeded)
        {
            return null;
        }

        await userManager.AddToRoleAsync(user, Roles.Customer);
        return tokenGenerator.CreateToken(new CurrentUser(user.Id, email, Roles.Customer));
    }

    public async Task<AuthenticationResponse?> LoginAsync(LoginCommand command, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(command.Email.Trim());
        if (user is null || !await userManager.CheckPasswordAsync(user, command.Password))
        {
            return null;
        }

        var roles = await userManager.GetRolesAsync(user);
        var role = roles.Contains(Roles.Admin, StringComparer.Ordinal) ? Roles.Admin : Roles.Customer;
        return tokenGenerator.CreateToken(new CurrentUser(user.Id, user.Email!, role));
    }

    public async Task<CurrentUser?> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user?.Email is null)
        {
            return null;
        }

        var roles = await userManager.GetRolesAsync(user);
        var role = roles.Contains(Roles.Admin, StringComparer.Ordinal) ? Roles.Admin : Roles.Customer;
        return new CurrentUser(user.Id, user.Email, role);
    }
}
