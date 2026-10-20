using CurrencyExchange.Application.DTOs;
using CurrencyExchange.Application.Exceptions;
using CurrencyExchange.Application.Interfaces;
using FluentValidation;

namespace CurrencyExchange.Application.Services;

public class AuthService(
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator tokenGenerator,
    IValidator<LoginRequest> validator) : IAuthService
{
    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        await validator.ValidateAndThrowAsync(request, ct);

        var user = await unitOfWork.Users.GetByUsernameAsync(request.Username.Trim().ToLowerInvariant(), ct);

        // Mismo mensaje si el usuario no existe o si la contraseña es incorrecta,
        // para no revelar qué usuarios existen (enumeración de usuarios).
        if (user is null || !passwordHasher.Verify(user.PasswordHash, request.Password))
            throw new UnauthorizedException("Usuario o contraseña incorrectos.");

        var (token, expiresAt) = tokenGenerator.Generate(user);

        return new LoginResponse(token, "Bearer", expiresAt, user.Username, user.Role);
    }
}
