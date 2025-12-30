using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WGCCI.Accounting.Api.Data;
using WGCCI.Accounting.Api.DTOs;

namespace WGCCI.Accounting.Api.Controllers;

[ApiController]
[Route("api/organizations")]
[Authorize(Roles = "ADMIN")]
public class OrganizationsController : ControllerBase
{
    private readonly AppDbContext _db;

    public OrganizationsController(AppDbContext db)
    {
        _db = db;
    }

    // GET /api/organizations
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Organizations
            .OrderBy(o => o.Name)
            .Select(o => new { o.Id, o.Name, o.BaseCurrency })
            .ToListAsync();

        return Ok(list);
    }

    // GET /api/organizations/5
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var org = await _db.Organizations
            .Where(o => o.Id == id)
            .Select(o => new { o.Id, o.Name, o.BaseCurrency })
            .FirstOrDefaultAsync();

        if (org == null) return NotFound();

        return Ok(org);
    }

    // POST /api/organizations
    [HttpPost]
    public async Task<IActionResult> Create(OrganizationCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("Name is required.");

        var org = new Organization
        {
            Name = dto.Name.Trim(),
            BaseCurrency = string.IsNullOrWhiteSpace(dto.BaseCurrency)
                ? "USD"
                : dto.BaseCurrency.Trim().ToUpperInvariant()
        };

        _db.Organizations.Add(org);
        await _db.SaveChangesAsync();

        var result = new { org.Id, org.Name, org.BaseCurrency };

        // Returns 201 + JSON and Location header
        return CreatedAtAction(nameof(GetById), new { id = org.Id }, result);
    }
}
