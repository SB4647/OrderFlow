using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OrderFlow.Orders.Application;

namespace OrderFlow.Orders.Infrastructure.Identity;

public sealed class JwtTokenGenerator(IOptions<JwtOptions> options) : IJwtTokenGenerator
{
    public AuthenticationResponse CreateToken(CurrentUser user)
    {
        var settings = options.Value;
        var expiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(settings.ExpirationMinutes);
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            settings.Issuer,
            settings.Audience,
            [
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            ],
            expires: expiresAtUtc.UtcDateTime,
            signingCredentials: credentials);

        return new AuthenticationResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAtUtc,
            user);
    }
}
