namespace CurrencyExchange.Application.Validators;

/// <summary>
/// Límites de negocio para montos y tasas. Evitan desbordes de decimal (monto × tasa)
/// y garantizan que el tipo de cambio inverso sea representable con 6 decimales.
/// </summary>
public static class ValidationLimits
{
    public const decimal MaxAmount = 1_000_000_000_000m;
    public const decimal MaxRate = 1_000_000m;
}
