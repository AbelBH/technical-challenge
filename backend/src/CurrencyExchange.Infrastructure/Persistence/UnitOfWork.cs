using CurrencyExchange.Application.Exceptions;
using CurrencyExchange.Application.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CurrencyExchange.Infrastructure.Persistence;

public class UnitOfWork(
    AppDbContext context,
    IExchangeRateRepository exchangeRates,
    IUserRepository users) : IUnitOfWork, IAsyncDisposable
{
    private IDbContextTransaction? _transaction;

    public IExchangeRateRepository ExchangeRates { get; } = exchangeRates;
    public IUserRepository Users { get; } = users;

    private const int SqliteConstraintErrorCode = 19;

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        try
        {
            return await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqliteException { SqliteErrorCode: SqliteConstraintErrorCode })
        {
            // Traduce el error técnico (índice único violado) a una excepción de negocio.
            throw new ConflictException("Ya existe un tipo de cambio para ese par de monedas.");
        }
    }

    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction is not null)
            throw new InvalidOperationException("Ya existe una transacción activa.");

        _transaction = await context.Database.BeginTransactionAsync(ct);
    }

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction is null)
            throw new InvalidOperationException("No hay una transacción activa para confirmar.");

        await _transaction.CommitAsync(ct);
        await DisposeTransactionAsync();
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction is null)
            return;

        await _transaction.RollbackAsync(ct);
        await DisposeTransactionAsync();
    }

    public async ValueTask DisposeAsync() => await DisposeTransactionAsync();

    private async Task DisposeTransactionAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
}
