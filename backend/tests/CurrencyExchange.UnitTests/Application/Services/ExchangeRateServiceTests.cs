using CurrencyExchange.Application.DTOs;
using CurrencyExchange.Application.Exceptions;
using CurrencyExchange.Application.Interfaces;
using CurrencyExchange.Application.Services;
using CurrencyExchange.Application.Validators;
using CurrencyExchange.Domain.Entities;
using FluentValidation;
using NSubstitute;

namespace CurrencyExchange.UnitTests.Application.Services;

public class ExchangeRateServiceTests
{
    private readonly IExchangeRateRepository _rates = Substitute.For<IExchangeRateRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ExchangeRateService _sut;

    public ExchangeRateServiceTests()
    {
        _unitOfWork.ExchangeRates.Returns(_rates);
        _sut = new ExchangeRateService(
            _unitOfWork,
            new CreateExchangeRateRequestValidator(),
            new UpdateExchangeRateRequestValidator());
    }

    // ---------- Create ----------

    [Fact]
    public async Task CreateAsync_SinInverso_GuardaUnRegistroYConfirmaLaTransaccion()
    {
        var result = await _sut.CreateAsync(new CreateExchangeRateRequest("usd", "pen", 3.75m));

        var dto = Assert.Single(result);
        Assert.Equal(("USD", "PEN", 3.75m), (dto.SourceCurrency, dto.TargetCurrency, dto.Rate));

        await _rates.Received(1).AddAsync(Arg.Any<ExchangeRate>(), Arg.Any<CancellationToken>());
        Received.InOrder(() =>
        {
            _unitOfWork.BeginTransactionAsync(Arg.Any<CancellationToken>());
            _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>());
            _unitOfWork.CommitTransactionAsync(Arg.Any<CancellationToken>());
        });
        await _unitOfWork.DidNotReceive().RollbackTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_ConInverso_GuardaAmbosRegistros()
    {
        var result = await _sut.CreateAsync(new CreateExchangeRateRequest("USD", "PEN", 3.75m, CreateInverse: true));

        Assert.Equal(2, result.Count);
        Assert.Equal(("PEN", "USD", 0.266667m), (result[1].SourceCurrency, result[1].TargetCurrency, result[1].Rate));
        await _rates.Received(2).AddAsync(Arg.Any<ExchangeRate>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_InversoDuplicado_HaceRollbackYNoGuardaNada()
    {
        _rates.ExistsAsync("PEN", "USD", Arg.Any<CancellationToken>()).Returns(true);

        await Assert.ThrowsAsync<ConflictException>(
            () => _sut.CreateAsync(new CreateExchangeRateRequest("USD", "PEN", 3.75m, CreateInverse: true)));

        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_SolicitudInvalida_NoAbreTransaccion()
    {
        await Assert.ThrowsAsync<ValidationException>(
            () => _sut.CreateAsync(new CreateExchangeRateRequest("USD", "USD", 1m)));

        await _unitOfWork.DidNotReceive().BeginTransactionAsync(Arg.Any<CancellationToken>());
    }

    // ---------- Get / Update / Delete ----------

    [Fact]
    public async Task GetByIdAsync_NoExiste_LanzaNotFoundException()
    {
        _rates.GetByIdAsync(99, Arg.Any<CancellationToken>()).Returns((ExchangeRate?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetByIdAsync(99));
    }

    [Fact]
    public async Task UpdateAsync_Existente_ActualizaTasaYGuarda()
    {
        var rate = new ExchangeRate("USD", "PEN", 3.75m);
        _rates.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(rate);

        var result = await _sut.UpdateAsync(1, new UpdateExchangeRateRequest(3.80m));

        Assert.Equal(3.80m, result.Rate);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_TasaInvalida_NoConsultaNiGuarda()
    {
        await Assert.ThrowsAsync<ValidationException>(
            () => _sut.UpdateAsync(1, new UpdateExchangeRateRequest(0m)));

        await _rates.DidNotReceiveWithAnyArgs().GetByIdAsync(default, default);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_Existente_EliminaYGuarda()
    {
        var rate = new ExchangeRate("USD", "PEN", 3.75m);
        _rates.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(rate);

        await _sut.DeleteAsync(1);

        _rates.Received(1).Remove(rate);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_NoExiste_LanzaNotFoundException()
    {
        _rates.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns((ExchangeRate?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteAsync(5));
        _rates.DidNotReceiveWithAnyArgs().Remove(default!);
    }
}
