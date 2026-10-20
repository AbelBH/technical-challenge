using CurrencyExchange.Application.Interfaces;
using CurrencyExchange.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CurrencyExchange.Infrastructure.Persistence;

/// <summary>
/// Crea los usuarios de demostración. Se hace por código (y no con HasData) porque
/// el hash de la contraseña lleva un salt aleatorio y HasData requiere valores fijos.
/// </summary>
public static class UserSeeder
{
    public static async Task SeedAsync(AppDbContext context, IPasswordHasher passwordHasher)
    {
        if (await context.Users.AnyAsync())
            return;

        var demoUsers = new[]
        {
            (Username: "admin", Password: "Admin123*", Role: Roles.Admin),
            (Username: "user", Password: "User123*", Role: Roles.User)
        };

        foreach (var demo in demoUsers)
        {
            var user = new User(demo.Username, demo.Role);
            user.SetPasswordHash(passwordHasher.Hash(demo.Password));
            context.Users.Add(user);
        }

        await context.SaveChangesAsync();
    }
}
