using CurrencyExchange.Application.DTOs;
using CurrencyExchange.Application.Exceptions;
using CurrencyExchange.Application.Interfaces;
using CurrencyExchange.Domain.Common;
using FluentValidation;

namespace CurrencyExchange.Application.Services;

public class CurrencyConversionService(
    IUnitOfWork unitOfWork,
    IValidator<ConversionRequest> validator) : ICurrencyConversionService
{
    public async Task<ConversionResponse> ConvertAsync(ConversionRequest request, CancellationToken ct = default)
    {
        await validator.ValidateAndThrowAsync(request, ct);

        var source = CurrencyCode.Normalize(request.SourceCurrency);
        var target = CurrencyCode.Normalize(request.TargetCurrency);

        // Misma moneda: el tipo de cambio es 1 y no se consulta la base de datos.
        if (source == target)
            return new ConversionResponse(request.Amount, request.Amount, source, target, 1m);

        var rate = await unitOfWork.ExchangeRates.GetByCurrenciesAsync(source, target, ct)
            ?? throw new NotFoundException($"No existe un tipo de cambio configurado para {source} → {target}.");

        return new ConversionResponse(
            Amount: request.Amount,
            ConvertedAmount: rate.Convert(request.Amount),
            SourceCurrency: source,
            TargetCurrency: target,
            ExchangeRate: rate.Rate);
    }
}
