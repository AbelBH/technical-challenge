using CurrencyExchange.Domain.Exceptions;

namespace CurrencyExchange.Domain.Common;

/// <summary>
/// Normaliza y valida códigos de moneda ISO 4217 (3 letras, ej: USD).
/// </summary>
public static class CurrencyCode
{
    public static string Normalize(string? currency)
    {
        var code = currency?.Trim().ToUpperInvariant() ?? string.Empty;

        if (code.Length != 3 || !code.All(char.IsAsciiLetter))
            throw new DomainException($"El código de moneda '{currency}' no es válido. Debe tener 3 letras, ej: USD.");

        return code;
    }
}
