using System.Text;
using CurrencyExchange.Api.Common;
using CurrencyExchange.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace CurrencyExchange.Api.Extensions;

public static class JwtAuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
            ?? throw new InvalidOperationException("Falta la sección de configuración 'Jwt'.");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Usa los nombres de claims tal como vienen en el token ("role", "unique_name").
                options.MapInboundClaims = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = settings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = settings.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key ?? string.Empty)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = JwtRegisteredClaimNames.UniqueName,
                    RoleClaimType = JwtClaimTypes.Role
                };

                // 401 y 403 con el mismo formato ProblemDetails que el resto de errores.
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();

                        var detail = context.AuthenticateFailure switch
                        {
                            SecurityTokenExpiredException => "El token ha expirado. Inicie sesión nuevamente.",
                            not null => "El token no es válido.",
                            null => "Se requiere un token de acceso (Authorization: Bearer <token>)."
                        };

                        await ProblemDetailsWriter.WriteAsync(context.HttpContext,
                            ProblemDetailsWriter.Create(StatusCodes.Status401Unauthorized, "No autenticado", detail));
                    },
                    OnForbidden = context =>
                        ProblemDetailsWriter.WriteAsync(context.HttpContext,
                            ProblemDetailsWriter.Create(StatusCodes.Status403Forbidden, "Acceso denegado",
                                "Su rol no tiene permiso para realizar esta operación."))
                };
            });

        services.AddAuthorization();

        return services;
    }
}
