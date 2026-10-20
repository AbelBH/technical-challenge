using CurrencyExchange.Application.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace CurrencyExchange.Infrastructure.Authentication;

/// <summary>
/// Usa el hasher de ASP.NET Core Identity (PBKDF2 con HMAC-SHA512, salt aleatorio e iteraciones),
/// en lugar de implementar criptografía propia.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<object> _hasher = new();
    private static readonly object User = new();

    public string Hash(string password) => _hasher.HashPassword(User, password);

    public bool Verify(string passwordHash, string password) =>
        _hasher.VerifyHashedPassword(User, passwordHash, password) != PasswordVerificationResult.Failed;
}
