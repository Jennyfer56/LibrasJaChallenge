using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LibrasJaChallenge.Auth;

public class JwtTokenService
{
    private readonly IConfiguration _config;

    public JwtTokenService(IConfiguration config) => _config = config;

    public string GenerateToken(string username, string role = "user")
    {
        var key      = _config["Jwt:Key"]      ?? throw new InvalidOperationException("Jwt:Key não configurada");
        var issuer   = _config["Jwt:Issuer"]   ?? "LibrasJa.API";
        var audience = _config["Jwt:Audience"] ?? "LibrasJa.Clients";
        var minutes  = int.Parse(_config["Jwt:ExpiresInMinutes"] ?? "120");

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer:    issuer,
            audience:  audience,
            claims:    claims,
            expires:   DateTime.UtcNow.AddMinutes(minutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public record LoginDto(string Username, string Password);
