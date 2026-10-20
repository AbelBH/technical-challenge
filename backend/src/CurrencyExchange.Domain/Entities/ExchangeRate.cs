using CurrencyExchange.Domain.Common;
using CurrencyExchange.Domain.Exceptions;

namespace CurrencyExchange.Domain.Entities;

/// <summary>
/// Tipo de cambio entre una moneda origen y una moneda destino.
/// Las reglas de negocio viven aquí: la entidad nunca queda en un estado inválido.
/// </summary>
public class ExchangeRate
{
    public int Id { get; private set; }
    public string SourceCurrency { get; private set; } = null!;
    public string TargetCurrency { get; private set; } = null!;
    public decimal Rate { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Requerido por EF Core para materializar la entidad.
    private ExchangeRate() { }

    public ExchangeRate(string sourceCurrency, string targetCurrency, decimal rate)
    {
        SourceCurrency = CurrencyCode.Normalize(sourceCurrency);
        TargetCurrency = CurrencyCode.Normalize(targetCurrency);

        if (SourceCurrency == TargetCurrency)
            throw new DomainException("La moneda origen y la moneda destino no pueden ser iguales.");

        SetRate(rate);
    }

    public void UpdateRate(decimal rate) => SetRate(rate);

    public decimal Convert(decimal amount)
    {
        if (amount < 0)
            throw new DomainException("El monto no puede ser negativo.");

        return Math.Round(amount * Rate, 4, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Crea el tipo de cambio inverso (ej: USD→PEN 3.75 ⇒ PEN→USD 0.266667).
    /// </summary>
    public ExchangeRate CreateInverse() =>
        new(TargetCurrency, SourceCurrency, Math.Round(1 / Rate, 6, MidpointRounding.AwayFromZero));

    private void SetRate(decimal rate)
    {
        if (rate <= 0)
            throw new DomainException("El tipo de cambio debe ser mayor a cero.");

        Rate = rate;
        UpdatedAt = DateTime.UtcNow;
    }
}
