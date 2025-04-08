namespace TravelAgent.Models
{
    public class TravelPlanResponse
    {
        public string City { get; set; } = string.Empty;
        public List<Location> Locations { get; set; } = new();
        public string TotalDistance { get; set; } = string.Empty;
        public string TotalTime { get; set; } = string.Empty;
    }
}
