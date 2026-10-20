namespace CurrencyExchange.Application.Exceptions;

/// <summary>
/// El recurso solicitado no existe (se traducirá a HTTP 404).
/// </summary>
public class NotFoundException(string message) : Exception(message);
