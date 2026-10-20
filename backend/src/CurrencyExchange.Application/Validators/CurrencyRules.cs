using FluentValidation;

namespace CurrencyExchange.Application.Validators;

/// <summary>
/// Reglas reutilizables para códigos de moneda.
/// </summary>
public static class CurrencyRules
{
    public static IRuleBuilderOptions<T, string> ValidCurrencyCode<T>(this IRuleBuilder<T, string> rule) =>
        rule
            .NotEmpty().WithMessage("{PropertyName} es obligatoria.")
            .Matches("^[A-Za-z]{3}$").WithMessage("{PropertyName} debe ser un código de 3 letras, ej: USD.");

    public static bool AreDifferent(string? source, string? target) =>
        !string.Equals(source?.Trim(), target?.Trim(), StringComparison.OrdinalIgnoreCase);
}
