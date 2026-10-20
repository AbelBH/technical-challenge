using CurrencyExchange.Application.Interfaces;
using CurrencyExchange.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CurrencyExchange.Infrastructure.Persistence.Repositories;

public class UserRepository(AppDbContext context) : IUserRepository
{
    public Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default) =>
        context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Username == username, ct);
}
