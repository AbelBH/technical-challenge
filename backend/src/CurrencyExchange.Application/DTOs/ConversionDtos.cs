namespace CurrencyExchange.Application.DTOs;

public record ConversionRequest(
    decimal Amount,
    string SourceCurrency,
    string TargetCurrency);

/// <param name="Amount">Monto original.</param>
/// <param name="ConvertedAmount">Monto con tipo de cambio aplicado.</param>
/// <param name="ExchangeRate">Tipo de cambio utilizado.</param>
public record ConversionResponse(
    decimal Amount,
    decimal ConvertedAmount,
    string SourceCurrency,
    string TargetCurrency,
    decimal ExchangeRate);
