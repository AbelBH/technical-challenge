using CurrencyExchange.Domain.Entities;

namespace CurrencyExchange.Application.Interfaces;

public interface IExchangeRateRepository
{
    Task<IReadOnlyList<ExchangeRate>> GetAllAsync(CancellationToken ct = default);
    Task<ExchangeRate?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ExchangeRate?> GetByCurrenciesAsync(string sourceCurrency, string targetCurrency, CancellationToken ct = default);
    Task<bool> ExistsAsync(string sourceCurrency, string targetCurrency, CancellationToken ct = default);
    Task AddAsync(ExchangeRate exchangeRate, CancellationToken ct = default);
    void Remove(ExchangeRate exchangeRate);
}
