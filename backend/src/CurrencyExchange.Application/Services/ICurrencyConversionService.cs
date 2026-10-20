using CurrencyExchange.Application.DTOs;

namespace CurrencyExchange.Application.Services;

public interface ICurrencyConversionService
{
    Task<ConversionResponse> ConvertAsync(ConversionRequest request, CancellationToken ct = default);
}
