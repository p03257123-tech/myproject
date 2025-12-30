using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using WGCCI.Accounting.Api.Data;
using WGCCI.Accounting.Api.DTOs;
using WGCCI.Accounting.Api.Services;
namespace WGCCI.Accounting.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db; private readonly IConfiguration _cfg;
    public AuthController(AppDbContext db, IConfiguration cfg)
    {
        _db = db;
        _cfg = cfg;
    }
    [HttpPost("register")]
    /////////
    ////[Authorize(Roles = "ADMIN")]


    public async Task<IActionResult> Register(RegisterDto dto)
    {
        if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
            return BadRequest("User exists");
        var u = new AppUser
        {
            OrgId = dto.OrgId,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };
        _db.Users.Add(u);
        await _db.SaveChangesAsync();
        foreach (var r in dto.Roles)
            _db.UserRoles.Add(new UserRole
            {
                UserId = u.Id,
                Role = Enum.Parse<Role>(r)
            });
        await _db.SaveChangesAsync();
        return Ok(new { u.Id, u.Email });
    }
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<TokenDto>> Login(LoginDto dto)
    {
        var u = await _db.Users.Include(x => x.Roles).FirstOrDefaultAsync(x => x.Email == dto.Email);
        if (u is null || !BCrypt.Net.BCrypt.Verify(dto.Password, u.PasswordHash))
            return Unauthorized();
        var token = TokenService.CreateToken(_cfg["Jwt:Key"] ?? "SuperUltraDevSecretKey_1234567890", _cfg["Jwt:Issuer"] ?? "wgcci", u.Id, u.OrgId, u.Roles.Select(r => r.Role.ToString()));

        return new TokenDto(token);
    }

}