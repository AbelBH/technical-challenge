using System.Text.Json;
using CurrencyExchange.Api.Common;
using CurrencyExchange.Application.Exceptions;
using CurrencyExchange.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyExchange.Api.Middleware;

/// <summary>
/// Captura cualquier excepción no controlada del pipeline y la convierte en una respuesta
/// uniforme con formato ProblemDetails (RFC 9457), con el código HTTP que corresponde.
/// </summary>
public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IHostEnvironment environment)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            if (context.Response.HasStarted)
            {
                logger.LogWarning("La respuesta ya había comenzado; no se puede escribir el error.");
                throw;
            }

            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var problem = exception switch
        {
            ValidationException ex => CreateValidationProblem(ex),
            DomainException ex => ProblemDetailsWriter.Create(StatusCodes.Status400BadRequest, "Regla de negocio no válida", ex.Message),
            UnauthorizedException ex => ProblemDetailsWriter.Create(StatusCodes.Status401Unauthorized, "No autorizado", ex.Message),
            NotFoundException ex => ProblemDetailsWriter.Create(StatusCodes.Status404NotFound, "Recurso no encontrado", ex.Message),
            ConflictException ex => ProblemDetailsWriter.Create(StatusCodes.Status409Conflict, "Conflicto", ex.Message),
            OperationCanceledException => ProblemDetailsWriter.Create(499, "Solicitud cancelada", "El cliente canceló la solicitud."),
            _ => ProblemDetailsWriter.Create(
                StatusCodes.Status500InternalServerError,
                "Error interno del servidor",
                // En producción no se exponen detalles internos.
                environment.IsDevelopment() ? exception.Message : "Ocurrió un error inesperado. Intente nuevamente.")
        };

        var status = problem.Status!.Value;

        // Errores del cliente (4xx) se registran como advertencia; errores del servidor (5xx) como error con stack trace.
        if (status >= 500)
            logger.LogError(exception, "Error no controlado en {Method} {Path}", context.Request.Method, context.Request.Path);
        else
            logger.LogWarning("Solicitud rechazada ({Status}) en {Method} {Path}: {Message}",
                status, context.Request.Method, context.Request.Path, exception.Message);

        await ProblemDetailsWriter.WriteAsync(context, problem);
    }

    private static ValidationProblemDetails CreateValidationProblem(ValidationException exception)
    {
        var errors = exception.Errors
            .GroupBy(e => JsonNamingPolicy.CamelCase.ConvertName(e.PropertyName))
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).Distinct().ToArray());

        return new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Uno o más datos de entrada no son válidos",
            Detail = "Revise el detalle en 'errors'."
        };
    }
}
