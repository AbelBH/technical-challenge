using CurrencyExchange.Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CurrencyExchange.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IExchangeRateService, ExchangeRateService>();
        services.AddScoped<ICurrencyConversionService, CurrencyConversionService>();
        services.AddScoped<IAuthService, AuthService>();

        // Registra todos los validadores (AbstractValidator<T>) de este ensamblado.
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
