namespace TravelAgent.Models
{
    public class Location
    {
        public string Name { get; set; } = string.Empty;
        public string DistanceFromPrevious { get; set; } = string.Empty;
        public string TimeToSpend { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
