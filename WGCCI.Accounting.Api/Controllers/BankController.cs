using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using WGCCI.Accounting.Api.DTOs;
using WGCCI.Accounting.Api.Services;

namespace WGCCI.Accounting.Api.Controllers;

[ApiController]
[Route("bank")]
public class BankController : ControllerBase
{
    private readonly BankService _svc;

    public BankController(BankService svc)
    {
        _svc = svc;
    }

    // ✅ Swagger-friendly file upload model
    public sealed class BankImportRequest
    {
        public IFormFile File { get; set; } = default!;
    }

    /// <summary>
    /// Import bank transactions from a CSV file.
    /// CSV format expected: Date,Description,Amount
    /// First row may be a header row.
    /// </summary>
    [HttpPost("import")]
    [Consumes("multipart/form-data")]
    [Authorize(Roles = "ADMIN,BURSAR,ACCOUNTANT")]
    public async Task<IActionResult> Import([FromForm] BankImportRequest request)
    {
        if (request.File == null || request.File.Length == 0)
            return BadRequest("File is required.");

        var orgClaim = User.FindFirst("org")?.Value;
        if (string.IsNullOrWhiteSpace(orgClaim) || !int.TryParse(orgClaim, out var orgId))
            return Unauthorized("Missing or invalid org claim.");

        var rows = new List<(DateOnly date, string description, decimal amount)>();

        using var reader = new StreamReader(request.File.OpenReadStream());
        string? line;
        bool firstLine = true;

        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            // Split CSV (simple split — matches your current implementation)
            // If your CSV can contain commas inside quotes, tell me and I'll upgrade this parser.
            var parts = line.Split(',');

            if (parts.Length < 3)
                continue;

            // Skip header row automatically if first column isn't a date
            if (firstLine)
            {
                firstLine = false;
                if (!DateOnly.TryParse(parts[0], out _))
                    continue;
            }

            if (!DateOnly.TryParse(parts[0], out var dt))
                continue;

            var desc = parts[1]?.Trim() ?? "";

            // Amount parsed using invariant culture (matches your code)
            if (!decimal.TryParse(parts[2], NumberStyles.Number, CultureInfo.InvariantCulture, out var amt))
                continue;

            rows.Add((dt, desc, amt));
        }

        var created = await _svc.ImportAsync(orgId, rows);
        return Ok(new { created });
    }

    [HttpPost("rules")]
    [Authorize(Roles = "ADMIN,ACCOUNTANT")]
    public async Task<IActionResult> AddRule([FromBody] BankRuleDto dto)
    {
        var orgClaim = User.FindFirst("org")?.Value;
        if (string.IsNullOrWhiteSpace(orgClaim) || !int.TryParse(orgClaim, out var orgId))
            return Unauthorized("Missing or invalid org claim.");

        var r = await _svc.AddRuleAsync(orgId, dto.Pattern, dto.AccountCode);
        return Ok(new { r.Id });
    }

    [HttpGet("suggestions")]
    [Authorize(Roles = "ADMIN,BURSAR,ACCOUNTANT,AUDITOR")]
    public async Task<IActionResult> Suggestions()
    {
        var orgClaim = User.FindFirst("org")?.Value;
        if (string.IsNullOrWhiteSpace(orgClaim) || !int.TryParse(orgClaim, out var orgId))
            return Unauthorized("Missing or invalid org claim.");

        var list = await _svc.SuggestionsAsync(orgId);
        return Ok(list);
    }
}