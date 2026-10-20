using CurrencyExchange.Application.Interfaces;
using CurrencyExchange.Infrastructure.Authentication;
using CurrencyExchange.Infrastructure.Persistence;
using CurrencyExchange.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CurrencyExchange.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Falta la cadena de conexión 'DefaultConnection'.");

        services.AddSingleton(new InMemoryDatabaseKeeper(connectionString));

        services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));

        services.AddScoped<IExchangeRateRepository, ExchangeRateRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Configuración JWT validada al arrancar: si falta o es insegura, la API no inicia.
        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection(JwtSettings.SectionName))
            .Validate(s => !string.IsNullOrWhiteSpace(s.Issuer) && !string.IsNullOrWhiteSpace(s.Audience),
                "Jwt:Issuer y Jwt:Audience son obligatorios.")
            .Validate(s => s.Key?.Length >= 32, "Jwt:Key debe tener al menos 32 caracteres.")
            .Validate(s => s.ExpirationMinutes > 0, "Jwt:ExpirationMinutes debe ser mayor a cero.")
            .ValidateOnStart();

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }

    /// <summary>
    /// Crea el esquema y carga los datos iniciales al arrancar la API.
    /// </summary>
    public static async Task InitializeDatabaseAsync(this IServiceProvider services)
    {
        // Fuerza la creación del singleton para abrir la conexión que mantiene viva la base.
        services.GetRequiredService<InMemoryDatabaseKeeper>();

        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        await context.Database.EnsureCreatedAsync();
        await UserSeeder.SeedAsync(context, passwordHasher);

        logger.LogInformation(
            "Base de datos en memoria inicializada con {Rates} tipos de cambio y {Users} usuarios.",
            await context.ExchangeRates.CountAsync(),
            await context.Users.CountAsync());
    }
}
