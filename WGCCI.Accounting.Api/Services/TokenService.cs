using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace WGCCI.Accounting.Api.Services;

public static class TokenService
{
    public static string CreateToken(
        string key,
        string issuer,
        int userId,
        int orgId,
        IEnumerable<string> roles)
    {
        var claims = new List<Claim>
        {
            new Claim("uid", userId.ToString()),
            new Claim("org", orgId.ToString())
        };

        claims.AddRange(
            roles.Select(r => new Claim(ClaimTypes.Role, r))
        );

        var creds = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256
        );

        var token = new JwtSecurityToken(
            issuer,
            null,
            claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
