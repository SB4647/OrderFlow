namespace OrderFlow.Orders.Infrastructure.Identity;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = "OrderFlow";

    public string Audience { get; init; } = "OrderFlow.Web";

    public string Key { get; init; } = string.Empty;

    public int ExpirationMinutes { get; init; } = 60;
}
