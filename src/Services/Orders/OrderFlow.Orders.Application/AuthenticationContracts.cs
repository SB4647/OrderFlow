namespace OrderFlow.Orders.Application;

public sealed record RegisterUserCommand(string Email, string Password);

public sealed record LoginCommand(string Email, string Password);

public sealed record CurrentUser(Guid Id, string Email, string Role);

public sealed record AuthenticationResponse(string AccessToken, DateTimeOffset ExpiresAtUtc, CurrentUser User);

public interface IAuthenticationService
{
    Task<AuthenticationResponse?> RegisterAsync(RegisterUserCommand command, CancellationToken cancellationToken);

    Task<AuthenticationResponse?> LoginAsync(LoginCommand command, CancellationToken cancellationToken);

    Task<CurrentUser?> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken);
}
