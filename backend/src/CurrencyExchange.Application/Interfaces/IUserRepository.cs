using CurrencyExchange.Domain.Entities;

namespace CurrencyExchange.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);
}
