using CurrencyExchange.Application.DTOs;
using CurrencyExchange.Application.Exceptions;
using CurrencyExchange.Application.Interfaces;
using CurrencyExchange.Application.Services;
using CurrencyExchange.Application.Validators;
using CurrencyExchange.Domain.Entities;
using FluentValidation;
using NSubstitute;

namespace CurrencyExchange.UnitTests.Application.Services;

public class CurrencyConversionServiceTests
{
    private readonly IExchangeRateRepository _rates = Substitute.For<IExchangeRateRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CurrencyConversionService _sut;

    public CurrencyConversionServiceTests()
    {
        _unitOfWork.ExchangeRates.Returns(_rates);
        // Se usa el validador real: así la prueba también cubre las reglas de entrada.
        _sut = new CurrencyConversionService(_unitOfWork, new ConversionRequestValidator());
    }

    [Fact]
    public async Task ConvertAsync_ConTasaExistente_DevuelveMontoConvertido()
    {
        _rates.GetByCurrenciesAsync("USD", "PEN", Arg.Any<CancellationToken>())
            .Returns(new ExchangeRate("USD", "PEN", 3.75m));

        var result = await _sut.ConvertAsync(new ConversionRequest(100m, "usd", "pen"));

        Assert.Equal(new ConversionResponse(100m, 375m, "USD", "PEN", 3.75m), result);
    }

    [Fact]
    public async Task ConvertAsync_MismaMoneda_DevuelveTasa1SinConsultarLaBase()
    {
        var result = await _sut.ConvertAsync(new ConversionRequest(50m, "USD", "usd"));

        Assert.Equal(50m, result.ConvertedAmount);
        Assert.Equal(1m, result.ExchangeRate);
        await _rates.DidNotReceiveWithAnyArgs().GetByCurrenciesAsync(default!, default!, default);
    }

    [Fact]
    public async Task ConvertAsync_SinTasaConfigurada_LanzaNotFoundException()
    {
        _rates.GetByCurrenciesAsync("USD", "EUR", Arg.Any<CancellationToken>())
            .Returns((ExchangeRate?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.ConvertAsync(new ConversionRequest(10m, "USD", "EUR")));
    }

    [Theory]
    [InlineData(-1, "USD", "PEN")]
    [InlineData(10, "US", "PEN")]
    [InlineData(10, "USD", "")]
    public async Task ConvertAsync_SolicitudInvalida_LanzaValidationException(decimal amount, string source, string target)
    {
        await Assert.ThrowsAsync<ValidationException>(
            () => _sut.ConvertAsync(new ConversionRequest(amount, source, target)));

        await _rates.DidNotReceiveWithAnyArgs().GetByCurrenciesAsync(default!, default!, default);
    }
}
