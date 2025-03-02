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
    public string GenerateToken(DateTime expires, string email, string roles)
    {
        var role = new List<string> { roles };
        return GenerateToken(expires, email, email, role);
    }

    public string GenerateToken(DateTime expires, string email, string owner, List<string> roles)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, email),
            new Claim("owner", owner),
        };
        foreach (var role in roles)
        {
            claims.Add(new Claim("tables", role));
        }

        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "SeuIssuer",
            audience: "SeuAudience",
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public bool CheckTableAccess(ClaimsPrincipal User, string table){
        var tables = GetAccess(User);
        if(tables.Contains(table)){
            return true;
        }
        return false;
    }

    public List<string> GetAccess(ClaimsPrincipal User){
        return User.Claims.Where(c => c.Type == "tables").ToList().Select(x => x.Value).ToList();
    }

    internal DateTime GetExpiration(ClaimsPrincipal User)
    {
        var expClaim = User.Claims.FirstOrDefault(c => c.Type == "exp")?.Value;
        if (expClaim != null && long.TryParse(expClaim, out var expUnix))
        {
            DateTime expirationTime  = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;
            return expirationTime;
        }
        return DateTime.MinValue;
    }
}

