using CurrencyExchange.Application.DTOs;
using FluentValidation;

namespace CurrencyExchange.Application.Validators;

public class ConversionRequestValidator : AbstractValidator<ConversionRequest>
{
    public ConversionRequestValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0).WithMessage("El monto no puede ser negativo.");

        RuleFor(x => x.SourceCurrency).ValidCurrencyCode().WithName("La moneda origen");
        RuleFor(x => x.TargetCurrency).ValidCurrencyCode().WithName("La moneda destino");
    }
}
