using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace TravelAgent.Services;

public interface IAuthService
{
    string GenerateToken(string username, string password);
}

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly string _testUsername;
    private readonly string _testPassword;

    public AuthService(IConfiguration configuration)
    {
        _configuration = configuration;
        _testUsername = configuration["TestCreds:Username"] ?? throw new ArgumentNullException("TestCreds:Username is not configured");
        _testPassword = configuration["TestCreds:Password"] ?? throw new ArgumentNullException("TestCreds:Password is not configured");
    }

    public string GenerateToken(string username, string password)
    {
        if (username != _testUsername || password != _testPassword)
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, "User")
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
} 