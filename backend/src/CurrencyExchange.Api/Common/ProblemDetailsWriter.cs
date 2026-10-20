using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyExchange.Api.Common;

/// <summary>
/// Escribe respuestas de error con formato ProblemDetails, compartido por el middleware
/// de excepciones y los eventos de autenticación (401/403).
/// </summary>
public static class ProblemDetailsWriter
{
    public static ProblemDetails Create(int status, string title, string detail) =>
        new() { Status = status, Title = title, Detail = detail };

    public static Task WriteAsync(HttpContext context, ProblemDetails problem)
    {
        problem.Instance ??= context.Request.Path;
        problem.Extensions["traceId"] = Activity.Current?.Id ?? context.TraceIdentifier;

        context.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        return context.Response.WriteAsJsonAsync(problem, problem.GetType(), options: null, contentType: "application/problem+json");
    }
}
