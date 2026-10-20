namespace CurrencyExchange.Application.DTOs;

public record ExchangeRateDto(
    int Id,
    string SourceCurrency,
    string TargetCurrency,
    decimal Rate,
    DateTime UpdatedAt);

/// <param name="CreateInverse">Si es true, crea también el tipo de cambio inverso en la misma transacción.</param>
public record CreateExchangeRateRequest(
    string SourceCurrency,
    string TargetCurrency,
    decimal Rate,
    bool CreateInverse = false);

public record UpdateExchangeRateRequest(decimal Rate);
