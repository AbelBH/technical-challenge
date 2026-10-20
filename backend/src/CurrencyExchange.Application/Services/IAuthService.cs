using CurrencyExchange.Application.DTOs;

namespace CurrencyExchange.Application.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
}
