using CurrencyExchange.Application.DTOs;
using FluentValidation;

namespace CurrencyExchange.Application.Validators;

public class CreateExchangeRateRequestValidator : AbstractValidator<CreateExchangeRateRequest>
{
    public CreateExchangeRateRequestValidator()
    {
        RuleFor(x => x.SourceCurrency).ValidCurrencyCode().WithName("La moneda origen");
        RuleFor(x => x.TargetCurrency).ValidCurrencyCode().WithName("La moneda destino");

        RuleFor(x => x)
            .Must(x => CurrencyRules.AreDifferent(x.SourceCurrency, x.TargetCurrency))
            .OverridePropertyName(nameof(CreateExchangeRateRequest.TargetCurrency))
            .WithMessage("La moneda origen y la moneda destino no pueden ser iguales.");

        RuleFor(x => x.Rate).ValidRate();
    }
}

public class UpdateExchangeRateRequestValidator : AbstractValidator<UpdateExchangeRateRequest>
{
    public UpdateExchangeRateRequestValidator()
    {
        RuleFor(x => x.Rate).ValidRate();
    }
}

internal static class RateRules
{
    public static IRuleBuilderOptions<T, decimal> ValidRate<T>(this IRuleBuilder<T, decimal> rule) =>
        rule
            .GreaterThan(0).WithMessage("El tipo de cambio debe ser mayor a cero.")
            .PrecisionScale(18, 6, ignoreTrailingZeros: true).WithMessage("El tipo de cambio admite como máximo 6 decimales.");
}
