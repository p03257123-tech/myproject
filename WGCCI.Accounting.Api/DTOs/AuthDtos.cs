namespace WGCCI.Accounting.Api.DTOs;

public record RegisterDto(
    int OrgId,
    string Email,
    string Password,
    IEnumerable<string> Roles
);

public record LoginDto(
    string Email,
    string Password
);

public record TokenDto(
    string AccessToken
);
