using CurrencyExchange.Domain.Entities;
using CurrencyExchange.Domain.Exceptions;

namespace CurrencyExchange.UnitTests.Domain;

public class ExchangeRateTests
{
    [Fact]
    public void Constructor_NormalizaLosCodigosDeMoneda()
    {
        var rate = new ExchangeRate(" usd ", "pen", 3.75m);

        Assert.Equal("USD", rate.SourceCurrency);
        Assert.Equal("PEN", rate.TargetCurrency);
        Assert.Equal(3.75m, rate.Rate);
    }

    [Fact]
    public void Constructor_MonedasIguales_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => new ExchangeRate("USD", "usd", 1m));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1.5)]
    public void Constructor_TasaNoPositiva_LanzaDomainException(decimal rate)
    {
        Assert.Throws<DomainException>(() => new ExchangeRate("USD", "PEN", rate));
    }

    [Theory]
    [InlineData("US")]
    [InlineData("USDT")]
    [InlineData("U5D")]
    [InlineData("")]
    public void Constructor_CodigoInvalido_LanzaDomainException(string code)
    {
        Assert.Throws<DomainException>(() => new ExchangeRate(code, "PEN", 3.75m));
    }

    [Theory]
    [InlineData(100, 375)]
    [InlineData(0, 0)]
    [InlineData(1.23456, 4.6296)] // 4.62960 → redondeo a 4 decimales
    public void Convert_AplicaLaTasaYRedondeaA4Decimales(decimal amount, decimal expected)
    {
        var rate = new ExchangeRate("USD", "PEN", 3.75m);

        Assert.Equal(expected, rate.Convert(amount));
    }

    [Fact]
    public void Convert_RedondeaAlejandoseDeCero()
    {
        var rate = new ExchangeRate("USD", "PEN", 1m);

        Assert.Equal(0.0001m, rate.Convert(0.00005m));
    }

    [Fact]
    public void Convert_MontoNegativo_LanzaDomainException()
    {
        var rate = new ExchangeRate("USD", "PEN", 3.75m);

        Assert.Throws<DomainException>(() => rate.Convert(-1m));
    }

    [Fact]
    public void CreateInverse_InvierteMonedasYCalculaTasaCon6Decimales()
    {
        var rate = new ExchangeRate("USD", "PEN", 3.75m);

        var inverse = rate.CreateInverse();

        Assert.Equal("PEN", inverse.SourceCurrency);
        Assert.Equal("USD", inverse.TargetCurrency);
        Assert.Equal(0.266667m, inverse.Rate);
    }

    [Fact]
    public void CreateInverse_TasaDemasiadoGrande_LanzaDomainException()
    {
        var rate = new ExchangeRate("USD", "XYZ", 10_000_000m); // 1/10M redondea a 0 con 6 decimales

        Assert.Throws<DomainException>(() => rate.CreateInverse());
    }

    [Fact]
    public void UpdateRate_CambiaLaTasaYLaFecha()
    {
        var rate = new ExchangeRate("USD", "PEN", 3.75m);
        var before = rate.UpdatedAt;

        rate.UpdateRate(3.80m);

        Assert.Equal(3.80m, rate.Rate);
        Assert.True(rate.UpdatedAt >= before);
    }

    [Fact]
    public void UpdateRate_TasaInvalida_NoModificaLaEntidad()
    {
        var rate = new ExchangeRate("USD", "PEN", 3.75m);

        Assert.Throws<DomainException>(() => rate.UpdateRate(0m));
        Assert.Equal(3.75m, rate.Rate);
    }
}
