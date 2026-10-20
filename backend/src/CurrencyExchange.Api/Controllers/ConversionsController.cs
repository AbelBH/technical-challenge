using CurrencyExchange.Application.DTOs;
using CurrencyExchange.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyExchange.Api.Controllers;

[ApiController]
[Route("api/conversions")]
[Produces("application/json")]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class ConversionsController(ICurrencyConversionService service) : ControllerBase
{
    /// <summary>
    /// Aplica el tipo de cambio a un monto.
    /// </summary>
    [HttpPost]
    [ProducesResponseType<ConversionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Convert(ConversionRequest request, CancellationToken ct) =>
        Ok(await service.ConvertAsync(request, ct));
}
