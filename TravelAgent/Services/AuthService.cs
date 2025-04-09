using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TravelAgent.Data;
using TravelAgent.Models.Data;
using TravelAgent.Utils;

namespace TravelAgent.Services;

public class AuthService : IAuthService
{
    private readonly TravelAgentDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly string _testUsername;
    private readonly string _testPassword;

    public AuthService(TravelAgentDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
        _testUsername = configuration["TestCreds:Username"] ?? throw new ArgumentNullException("TestCreds:Username is not configured");
        _testPassword = configuration["TestCreds:Password"] ?? throw new ArgumentNullException("TestCreds:Password is not configured");
    }

    public async Task<bool> RegisterAsync(string username, string password)
    {
        if (await _context.Users.AnyAsync(u => u.Username == username))
            return false;

        var user = new User
        {
            Username = username,
            PasswordHash = PasswordHasher.Hash(password)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<string?> LoginAsync(string username, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null || !PasswordHasher.Verify(password, user.PasswordHash))
            return null;



        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);

        // Create JWT token
        //var tokenHandler = new JwtSecurityTokenHandler();
        //var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);
        //var tokenDescriptor = new SecurityTokenDescriptor
        //{
        //    Subject = new ClaimsIdentity(new[]
        //    {
        //        new Claim(ClaimTypes.Name, user.Username),
        //        new Claim(ClaimTypes.Role, user.Role)
        //    }),
        //    Expires = DateTime.UtcNow.AddHours(1),
        //    Issuer = _config["Jwt:Issuer"],
        //    Audience = _config["Jwt:Audience"],
        //    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
        //};

        //var token = tokenHandler.CreateToken(tokenDescriptor);
        //return tokenHandler.WriteToken(token);
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