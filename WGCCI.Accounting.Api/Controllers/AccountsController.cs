using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WGCCI.Accounting.Api.Data;
using WGCCI.Accounting.Api.DTOs;

namespace WGCCI.Accounting.Api.Controllers;

[ApiController]
[Route("accounts")]
[Authorize(Roles = "ADMIN,BURSAR,ACCOUNTANT,AUDITOR")]
public class AccountsController : ControllerBase
{
    private readonly AppDbContext _db;

    public AccountsController(AppDbContext db)
    {
        _db = db;
    }

    private int? GetOrgIdFromUser()
    {
        var orgClaim = User.FindFirst("org")?.Value;
        if (string.IsNullOrWhiteSpace(orgClaim)) return null;
        return int.TryParse(orgClaim, out var orgId) ? orgId : null;
    }

    // GET /accounts
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var orgId = GetOrgIdFromUser();
        if (orgId == null) return Unauthorized("Missing org claim.");

        var accounts = await _db.Accounts
            .Where(a => a.OrgId == orgId.Value)
            .OrderBy(a => a.Code)
            .Select(a => new
            {
                a.Id,
                a.Code,
                a.Name,
                a.Type,
                a.Currency
            })
            .ToListAsync();

        return Ok(accounts);
    }

    // POST /accounts
    [HttpPost]
    public async Task<IActionResult> Create(AccountCreateDto dto)
    {
        var orgId = GetOrgIdFromUser();
        if (orgId == null) return Unauthorized("Missing org claim.");

        if (string.IsNullOrWhiteSpace(dto.Code))
            return BadRequest("Code is required.");
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("Name is required.");

        var exists = await _db.Accounts
            .AnyAsync(a => a.OrgId == orgId.Value && a.Code == dto.Code);
        if (exists)
            return Conflict($"Account with code {dto.Code} already exists for this organization.");

        var account = new Account
        {
            OrgId = orgId.Value,
            Code = dto.Code.Trim(),
            Name = dto.Name.Trim(),
            Type = dto.Type,
            Currency = string.IsNullOrWhiteSpace(dto.Currency)
                ? null
                : dto.Currency.Trim().ToUpperInvariant()
        };

        _db.Accounts.Add(account);
        await _db.SaveChangesAsync();

        var result = new
        {
            account.Id,
            account.Code,
            account.Name,
            account.Type,
            account.Currency
        };

        return Created(string.Empty, result);
    }
}
