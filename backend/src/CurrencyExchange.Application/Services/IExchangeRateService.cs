using CurrencyExchange.Application.DTOs;

namespace CurrencyExchange.Application.Services;

public interface IExchangeRateService
{
    Task<IReadOnlyList<ExchangeRateDto>> GetAllAsync(CancellationToken ct = default);
    Task<ExchangeRateDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<ExchangeRateDto>> CreateAsync(CreateExchangeRateRequest request, CancellationToken ct = default);
    Task<ExchangeRateDto> UpdateAsync(int id, UpdateExchangeRateRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
