using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WGCCI.Accounting.Api.DTOs;
using WGCCI.Accounting.Api.Services;

namespace WGCCI.Accounting.Api.Controllers;

[ApiController]
[Route("fx")]
public class FXController : ControllerBase
{
    private readonly FXService _svc;

    public FXController(FXService svc)
    {
        _svc = svc;
    }

    [HttpPost("rate")]
    [Authorize(Roles = "ADMIN,ACCOUNTANT")]
    public async Task<IActionResult> UpsertRate(RateDto dto)
    {
        var orgId = int.Parse(User.FindFirst("org")!.Value);

        var row = await _svc.UpsertRateAsync(
            orgId,
            dto.Date,
            dto.FromCurrency,
            dto.ToCurrency,
            dto.Rate
        );

        return Ok(new { row.Id });
    }

    [HttpPost("revalue")]
    [Authorize(Roles = "ADMIN,ACCOUNTANT")]
    public async Task<IActionResult> Revalue(DateOnly asOf, string revalEquityCode)
    {
        var orgId = int.Parse(User.FindFirst("org")!.Value);

        var result = await _svc.RevalueAsync(
            orgId,
            asOf,
            revalEquityCode
        );

        return Ok(result);
    }
}
