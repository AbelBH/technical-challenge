using CurrencyExchange.Application.DTOs;
using CurrencyExchange.Application.Exceptions;
using CurrencyExchange.Application.Interfaces;
using CurrencyExchange.Application.Mappings;
using CurrencyExchange.Domain.Entities;
using FluentValidation;

namespace CurrencyExchange.Application.Services;

public class ExchangeRateService(
    IUnitOfWork unitOfWork,
    IValidator<CreateExchangeRateRequest> createValidator,
    IValidator<UpdateExchangeRateRequest> updateValidator) : IExchangeRateService
{
    public async Task<IReadOnlyList<ExchangeRateDto>> GetAllAsync(CancellationToken ct = default)
    {
        var rates = await unitOfWork.ExchangeRates.GetAllAsync(ct);
        return rates.Select(r => r.ToDto()).ToList();
    }

    public async Task<ExchangeRateDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var rate = await GetExistingAsync(id, ct);
        return rate.ToDto();
    }

    /// <summary>
    /// Crea un tipo de cambio y, opcionalmente, su inverso.
    /// Se usa una transacción explícita para que la verificación de duplicados y las
    /// inserciones sean una sola unidad: se guardan ambos o ninguno.
    /// </summary>
    public async Task<IReadOnlyList<ExchangeRateDto>> CreateAsync(CreateExchangeRateRequest request, CancellationToken ct = default)
    {
        await createValidator.ValidateAndThrowAsync(request, ct);

        // El constructor normaliza (ej: "usd" → "USD") y protege las reglas del dominio.
        var rate = new ExchangeRate(request.SourceCurrency, request.TargetCurrency, request.Rate);
        var toCreate = new List<ExchangeRate> { rate };

        if (request.CreateInverse)
            toCreate.Add(rate.CreateInverse());

        await unitOfWork.BeginTransactionAsync(ct);
        try
        {
            foreach (var item in toCreate)
            {
                if (await unitOfWork.ExchangeRates.ExistsAsync(item.SourceCurrency, item.TargetCurrency, ct))
                    throw new ConflictException($"Ya existe un tipo de cambio {item.SourceCurrency} → {item.TargetCurrency}.");

                await unitOfWork.ExchangeRates.AddAsync(item, ct);
            }

            await unitOfWork.SaveChangesAsync(ct);
            await unitOfWork.CommitTransactionAsync(ct);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }

        return toCreate.Select(r => r.ToDto()).ToList();
    }

    // Update y Delete modifican un solo registro: SaveChanges ya es atómico,
    // por lo que no se necesita una transacción explícita.
    public async Task<ExchangeRateDto> UpdateAsync(int id, UpdateExchangeRateRequest request, CancellationToken ct = default)
    {
        await updateValidator.ValidateAndThrowAsync(request, ct);

        var rate = await GetExistingAsync(id, ct);

        rate.UpdateRate(request.Rate);
        await unitOfWork.SaveChangesAsync(ct);

        return rate.ToDto();
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var rate = await GetExistingAsync(id, ct);

        unitOfWork.ExchangeRates.Remove(rate);
        await unitOfWork.SaveChangesAsync(ct);
    }

    private async Task<ExchangeRate> GetExistingAsync(int id, CancellationToken ct) =>
        await unitOfWork.ExchangeRates.GetByIdAsync(id, ct)
        ?? throw new NotFoundException($"No existe el tipo de cambio con Id {id}.");
}
