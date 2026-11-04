using CurrencyExchange.Application.DTOs;
using CurrencyExchange.Application.Validators;
using FluentValidation.TestHelper;

namespace CurrencyExchange.UnitTests.Application.Validators;

public class ConversionRequestValidatorTests
{
    private readonly ConversionRequestValidator _validator = new();

    [Theory]
    [InlineData(0)]
    [InlineData(100.50)]
    public void MontoValido_NoTieneErrores(decimal amount)
    {
        var result = _validator.TestValidate(new ConversionRequest(amount, "USD", "PEN"));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void MontoNegativo_TieneError()
    {
        _validator.TestValidate(new ConversionRequest(-0.01m, "USD", "PEN"))
            .ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void MontoSobreElLimite_TieneError()
    {
        _validator.TestValidate(new ConversionRequest(ValidationLimits.MaxAmount + 1, "USD", "PEN"))
            .ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void MontoEnElLimite_EsValido()
    {
        _validator.TestValidate(new ConversionRequest(ValidationLimits.MaxAmount, "USD", "PEN"))
            .ShouldNotHaveValidationErrorFor(x => x.Amount);
    }

    [Theory]
    [InlineData("")]
    [InlineData("US")]
    [InlineData("12A")]
    public void MonedaOrigenInvalida_TieneError(string source)
    {
        _validator.TestValidate(new ConversionRequest(10m, source, "PEN"))
            .ShouldHaveValidationErrorFor(x => x.SourceCurrency);
    }
}

public class CreateExchangeRateRequestValidatorTests
{
    private readonly CreateExchangeRateRequestValidator _validator = new();

    [Fact]
    public void SolicitudValida_NoTieneErrores()
    {
        _validator.TestValidate(new CreateExchangeRateRequest("USD", "PEN", 3.756789m))
            .ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void MonedasIguales_SinImportarMayusculas_TieneError()
    {
        _validator.TestValidate(new CreateExchangeRateRequest("usd", " USD", 1m))
            .ShouldHaveValidationErrorFor(x => x.TargetCurrency);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1_000_000.01)]
    [InlineData(1.1234567)] // 7 decimales
    public void TasaInvalida_TieneError(decimal rate)
    {
        _validator.TestValidate(new CreateExchangeRateRequest("USD", "PEN", rate))
            .ShouldHaveValidationErrorFor(x => x.Rate);
    }

    [Fact]
    public void TasaConCerosFinales_SeIgnoranParaLaPrecision()
    {
        _validator.TestValidate(new CreateExchangeRateRequest("USD", "PEN", 3.750000000m))
            .ShouldNotHaveValidationErrorFor(x => x.Rate);
    }
}

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    [Fact]
    public void CamposVacios_TienenError()
    {
        var result = _validator.TestValidate(new LoginRequest("", ""));

        result.ShouldHaveValidationErrorFor(x => x.Username);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}
