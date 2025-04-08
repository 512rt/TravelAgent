using Microsoft.AspNetCore.Mvc;

namespace TravelAgent.Controllers;

[ApiController]
[Route("[controller]")]
public class TravelAgentController : ControllerBase
{
    private readonly ILogger<TravelAgentController> _logger;

    public TravelAgentController(ILogger<TravelAgentController> logger)
    {
        _logger = logger;
    }

    [HttpGet("trip")]
    public ActionResult<TripResponse> GetTrip()
    {
        var response = new TripResponse
        {
            Id = Guid.NewGuid(),
            Destination = "Sample Destination",
            StartDate = DateTime.Now.AddDays(7),
            EndDate = DateTime.Now.AddDays(14),
            Price = 999.99m
        };

        return Ok(response);
    }
}

public class TripResponse
{
    public Guid Id { get; set; }
    public string Destination { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Price { get; set; }
} 