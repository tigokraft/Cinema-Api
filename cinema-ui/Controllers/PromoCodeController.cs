using Microsoft.AspNetCore.Mvc;
using cinema_ui.Services;

namespace cinema_ui.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PromoCodeController : ControllerBase
{
    private readonly AdminApiService _adminApi;

    public PromoCodeController(AdminApiService adminApi)
    {
        _adminApi = adminApi;
    }

    [HttpGet("validate/{code}")]
    public async Task<IActionResult> Validate(string code, [FromQuery] decimal? purchaseAmount)
    {
        var result = await _adminApi.ValidatePromoCodeAsync(code, purchaseAmount);
        return Ok(result);
    }
}
