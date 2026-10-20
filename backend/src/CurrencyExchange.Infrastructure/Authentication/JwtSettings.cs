namespace CurrencyExchange.Infrastructure.Authentication;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = null!;
    public string Audience { get; init; } = null!;

    /// <summary>Clave secreta de firma (HMAC-SHA256). Mínimo 32 caracteres. En producción va en variables de entorno.</summary>
    public string Key { get; init; } = null!;

    public int ExpirationMinutes { get; init; } = 60;
}
