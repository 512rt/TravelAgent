using Microsoft.AspNetCore.Mvc;
using TravelAgent.Models;
using TravelAgent.Services;

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

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        try
        {
            var token = _authService.GenerateToken(request.Username, request.Password);
            return Ok(new { token });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "Invalid credentials" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, new { message = "An error occurred during login" });
        }
    }


    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] LoginRequest model)
    {
        try
        {
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

    [HttpPost("login2")]
    public async Task<IActionResult> Login2([FromBody] LoginRequest model)
    {
        try
        {
            var token = await _authService.LoginAsync(model.Username, model.Password);
            if (token == null) return Unauthorized("Invalid credentials");
            return Ok(new { token });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Login2");
            throw;
        }
    }
}