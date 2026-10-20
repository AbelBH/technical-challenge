using System.Diagnostics;
using CurrencyExchange.Api.Extensions;
using CurrencyExchange.Api.Middleware;
using CurrencyExchange.Application;
using CurrencyExchange.Infrastructure;
using CurrencyExchange.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // Errores de formato del cuerpo (JSON inválido, tipos incorrectos) con el mismo formato que el middleware.
        options.InvalidModelStateResponseFactory = context =>
        {
            var problem = new ValidationProblemDetails(context.ModelState)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "La solicitud tiene un formato inválido",
                Detail = "Verifique que el cuerpo sea un JSON válido y que los tipos de datos sean correctos.",
                Instance = context.HttpContext.Request.Path
            };
            problem.Extensions["traceId"] = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;

            return new BadRequestObjectResult(problem) { ContentTypes = { "application/problem+json" } };
        };
    })
    // No exponer en los errores los nombres internos de clases .NET.
    .AddJsonOptions(options => options.AllowInputFormatterExceptionMessages = false);
builder.Services.AddJwtAuthentication(builder.Configuration);

// CORS: permite que el front Angular (otro origen/puerto) llame a la API desde el navegador.
const string FrontendCorsPolicy = "Frontend";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddPolicy(FrontendCorsPolicy, policy =>
    policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddOpenApi(options => options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());
builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>();

var app = builder.Build();

await app.Services.InitializeDatabaseAsync();

// Primero en el pipeline para capturar las excepciones de todo lo que viene después.
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Swagger siempre en Development; en otros entornos (ej: Docker) solo si se habilita con Swagger:Enabled=true.
if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Swagger:Enabled"))
{
    // Documento OpenAPI nativo de .NET (/openapi/v1.json) visualizado con Swagger UI (/swagger).
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "CurrencyExchange API v1"));
}

// Sin UseHttpsRedirection: la API corre en HTTP (localmente y dentro de Docker). En producción
// el HTTPS se termina en el proxy inverso / balanceador. Además, un redirect rompe el preflight de CORS.

// CORS antes de la autenticación para que las peticiones preflight (OPTIONS) no exijan token.
app.UseCors(FrontendCorsPolicy);

// Orden importante: primero se identifica al usuario (token) y luego se verifica su permiso (rol).
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Endpoint público para verificar que la API (y su base de datos) responden. Usado por Docker.
app.MapHealthChecks("/health").AllowAnonymous();

app.Run();
