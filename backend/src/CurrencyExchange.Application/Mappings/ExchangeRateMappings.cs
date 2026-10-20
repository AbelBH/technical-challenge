using CurrencyExchange.Application.DTOs;
using CurrencyExchange.Domain.Entities;

namespace CurrencyExchange.Application.Mappings;

public static class ExchangeRateMappings
{
    public static ExchangeRateDto ToDto(this ExchangeRate entity) =>
        new(entity.Id, entity.SourceCurrency, entity.TargetCurrency, entity.Rate, entity.UpdatedAt);
}
