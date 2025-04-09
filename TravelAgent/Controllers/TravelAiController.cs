using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelAgent.Models;
using TravelAgent.ServiceClients;

namespace TravelAgent.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TravelAiController : ControllerBase
{
    private readonly ITravelAiClient _travelAiClient;
    private readonly ILogger<TravelAiController> _logger;

    public TravelAiController(ITravelAiClient travelAiClient, ILogger<TravelAiController> logger)
    {
        _travelAiClient = travelAiClient;
        _logger = logger;
    }

    [HttpGet("plan/{city}")]
    public async Task<IActionResult> GetTravelPlan(string city)
    {
        try
        {
            _logger.LogTrace($"GetTravelPlan called with city: {city}");

            if (string.IsNullOrWhiteSpace(city))
            {
                return BadRequest("City name cannot be empty");
            }

            TravelPlanResponse plan = await _travelAiClient.GetTravelPlanAsync(city);
            return Ok(plan);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating travel plan for {City}", city);
            
            // Return appropriate status code based on the exception
            if (ex.Message.Contains("Hugging Face API error"))
            {
                return StatusCode(502, new { error = "Error communicating with AI service", details = ex.Message });
            }
            else if (ex.Message.Contains("Failed to parse"))
            {
                return StatusCode(500, new { error = "Error processing AI response", details = ex.Message });
            }
            
            return StatusCode(500, new { error = "An unexpected error occurred", details = ex.Message });
        }
    }
} 