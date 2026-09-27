using OrderFlow.Orders.Application;

namespace OrderFlow.Orders.Infrastructure.Identity;

public interface IJwtTokenGenerator
{
    AuthenticationResponse CreateToken(CurrentUser user);
}
