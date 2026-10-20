namespace CurrencyExchange.Application.Exceptions;

/// <summary>
/// La operación entra en conflicto con el estado actual, ej: par de monedas duplicado (se traducirá a HTTP 409).
/// </summary>
public class ConflictException(string message) : Exception(message);
