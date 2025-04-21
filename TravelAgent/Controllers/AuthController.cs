using Microsoft.AspNetCore.Mvc;
using TravelAgent.Models;
using TravelAgent.Services.Interfaces;

namespace TravelAgent.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] LoginRequest model)
    {
        try
        {
            _logger.LogTrace($"Register called with Register_Username: {model.Username}");
            var success = await _authService.RegisterAsync(model.Username, model.Password);
            if (!success) return BadRequest("Username already exists");
            return Ok("Registered successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Register");
            throw;
        }
        
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest model)
    {
        try
        {
            _logger.LogTrace($"Login called with Login_Username: {model.Username}");
            var token = await _authService.LoginAsync(model.Username, model.Password);
            if (token == null) return Unauthorized("Invalid credentials");
            return Ok(new { token });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Login");
            throw;
        }
    }
}