namespace CurrencyExchange.Application.Interfaces;

/// <summary>
/// Agrupa los cambios de una operación y controla la transacción,
/// para que la capa de aplicación no dependa de EF Core directamente.
/// </summary>
public interface IUnitOfWork
{
    IExchangeRateRepository ExchangeRates { get; }
    IUserRepository Users { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);
}
