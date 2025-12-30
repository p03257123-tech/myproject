using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WGCCI.Accounting.Api.DTOs;
using WGCCI.Accounting.Api.Services;

namespace WGCCI.Accounting.Api.Controllers;

[ApiController]
[Route("tax")]
public class TaxController : ControllerBase
{
    private readonly TaxService _svc;

    public TaxController(TaxService svc)
    {
        _svc = svc;
    }

    [HttpPost("codes")]
    [Authorize(Roles = "ADMIN,ACCOUNTANT")]
    public async Task<IActionResult> CreateCode(TaxCodeDto dto)
    {
        var orgId = int.Parse(User.FindFirst("org")!.Value);

        var tc = await _svc.CreateTaxCodeAsync(
            orgId,
            dto.Name,
            dto.Rate,
            dto.Type
        );

        return Ok(new { tc.Id });
    }
}
