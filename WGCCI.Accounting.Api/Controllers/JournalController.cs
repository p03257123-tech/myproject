using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WGCCI.Accounting.Api.DTOs;
using WGCCI.Accounting.Api.Services;
using System.Linq;

namespace WGCCI.Accounting.Api.Controllers;

[ApiController]
[Route("journal")]
public class JournalController : ControllerBase
{
    private readonly AccountingService _svc;

    public JournalController(AccountingService svc)
    {
        _svc = svc;
    }

    [HttpPost("entries")]
    [Authorize(Roles = "ADMIN,BURSAR,ACCOUNTANT")]
    public async Task<IActionResult> PostEntry(JournalEntryDto dto)
    {
        var orgId = int.Parse(User.FindFirst("org")!.Value);

        try
        {
            var je = await _svc.PostEntryAsync(
                orgId,
                dto.Date,
                dto.Memo,
                dto.Lines.Select(l => (l.AccountCode, l.Debit, l.Credit))
            );

            return Ok(new { je.Id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
