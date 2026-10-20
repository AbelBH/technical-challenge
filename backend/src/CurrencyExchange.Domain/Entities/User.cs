using CurrencyExchange.Domain.Exceptions;

namespace CurrencyExchange.Domain.Entities;

/// <summary>
/// Usuario de la API. Solo se guarda el hash de la contraseña, nunca el texto plano.
/// </summary>
public class User
{
    public int Id { get; private set; }
    public string Username { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string Role { get; private set; } = null!;

    private User() { }

    public User(string username, string role)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new DomainException("El nombre de usuario es obligatorio.");

        if (!Roles.All.Contains(role))
            throw new DomainException($"El rol '{role}' no es válido.");

        Username = username.Trim().ToLowerInvariant();
        Role = role;
    }

    public void SetPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("El hash de la contraseña es obligatorio.");

        PasswordHash = passwordHash;
    }
}
