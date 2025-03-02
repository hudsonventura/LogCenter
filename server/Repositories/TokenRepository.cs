using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace server.Repositories;

public class TokenRepository
{
    private SymmetricSecurityKey _key;
    public TokenRepository(){

        string secretJWTKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? throw new Exception("JWT_KEY not found");
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretJWTKey));
    }
    public string GenerateToken(string username, string role)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role)
        };

        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "SeuIssuer",
            audience: "SeuAudience",
            claims: claims,
            expires: DateTime.Now,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
