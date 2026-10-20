using CurrencyExchange.Domain.Entities;

namespace CurrencyExchange.Application.Interfaces;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAt) Generate(User user);
}
