using CurrencyExchange.Application.Interfaces;
using CurrencyExchange.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CurrencyExchange.Infrastructure.Persistence.Repositories;

public class ExchangeRateRepository(AppDbContext context) : IExchangeRateRepository
{
    public async Task<IReadOnlyList<ExchangeRate>> GetAllAsync(CancellationToken ct = default) =>
        await context.ExchangeRates
            .AsNoTracking()
            .OrderBy(x => x.SourceCurrency)
            .ThenBy(x => x.TargetCurrency)
            .ToListAsync(ct);

    public Task<ExchangeRate?> GetByIdAsync(int id, CancellationToken ct = default) =>
        context.ExchangeRates.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<ExchangeRate?> GetByCurrenciesAsync(string sourceCurrency, string targetCurrency, CancellationToken ct = default) =>
        context.ExchangeRates.FirstOrDefaultAsync(
            x => x.SourceCurrency == sourceCurrency && x.TargetCurrency == targetCurrency, ct);

    public Task<bool> ExistsAsync(string sourceCurrency, string targetCurrency, CancellationToken ct = default) =>
        context.ExchangeRates.AnyAsync(
            x => x.SourceCurrency == sourceCurrency && x.TargetCurrency == targetCurrency, ct);

    public async Task AddAsync(ExchangeRate exchangeRate, CancellationToken ct = default) =>
        await context.ExchangeRates.AddAsync(exchangeRate, ct);

    public void Remove(ExchangeRate exchangeRate) =>
        context.ExchangeRates.Remove(exchangeRate);
}
