namespace TravelAgent.ServiceClients;

public interface ITravelAiClient
{
    Task<TravelPlanResponse> GetTravelPlanAsync(string city);
} 