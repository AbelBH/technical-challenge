namespace CurrencyExchange.Application.Exceptions;

/// <summary>
/// Credenciales inválidas (se traducirá a HTTP 401).
/// </summary>
public class UnauthorizedException(string message) : Exception(message);
