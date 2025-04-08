using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;

namespace TravelAgent.ServiceClients;

public class TravelAiClient : ITravelAiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<TravelAiClient> _logger;
    private readonly string _modelEndpoint = "https://api-inference.huggingface.co/models/mistralai/Mistral-7B-Instruct-v0.1";

    public TravelAiClient(IConfiguration configuration, ILogger<TravelAiClient> logger)
    {
        _httpClient = new HttpClient();
        _apiKey = configuration["HuggingFace:ApiKey"] ?? throw new ArgumentNullException("HuggingFace:ApiKey is not configured");
        _logger = logger;
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }

    public async Task<TravelPlanResponse> GetTravelPlanAsync(string city)
    {
        try
        {
            _logger.LogInformation("Generating travel plan for city: {City}", city);
            
            var prompt = GenerateTravelPrompt(city);
            var requestBody = new
            {
                inputs = prompt,
                parameters = new
                {
                    max_new_tokens = 1000,
                    temperature = 0.7,
                    top_p = 0.9,
                    return_full_text = false
                }
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            _logger.LogDebug("Sending request to Hugging Face API");
            var response = await _httpClient.PostAsync(_modelEndpoint, content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Hugging Face API returned error: {StatusCode} - {Content}", 
                    response.StatusCode, errorContent);
                
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new Exception("Invalid or missing Hugging Face API key. Please check your configuration.");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    throw new Exception("Model not found. Please check the model endpoint.");
                }
                
                throw new Exception($"Hugging Face API error: {response.StatusCode} - {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Received response from Hugging Face API: {Response}", responseContent);

            // Parse the response as an array of HuggingFaceResponse
            var aiResponses = JsonSerializer.Deserialize<List<HuggingFaceResponse>>(responseContent);
            if (aiResponses == null || !aiResponses.Any() || string.IsNullOrEmpty(aiResponses[0].GeneratedText))
            {
                throw new Exception("Invalid response from Hugging Face API");
            }

            return ParseTravelPlanResponse(aiResponses[0].GeneratedText);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating travel plan for {City}", city);
            throw;
        }
    }

    private string GenerateTravelPrompt(string city)
    {
        return $@"<s>[INST] Create a detailed one-day travel plan for {city}. 
        The response should be in JSON format with the following structure:
        {{
            ""city"": ""{city}"",
            ""locations"": [
                {{
                    ""name"": ""Location Name"",
                    ""distanceFromPrevious"": ""Distance in km"",
                    ""timeToSpend"": ""Time in hours"",
                    ""description"": ""Brief description""
                }}
            ],
            ""totalDistance"": ""Total distance in km"",
            ""totalTime"": ""Total time in hours""
        }}
        Include 5-7 popular tourist attractions, considering reasonable travel times between locations.
        Make sure the total time adds up to a full day (8-10 hours).
        [/INST]</s>";
    }

    private TravelPlanResponse ParseTravelPlanResponse(string aiResponse)
    {
        try
        {
            _logger.LogDebug("Parsing AI response: {Response}", aiResponse);
            return JsonSerializer.Deserialize<TravelPlanResponse>(aiResponse) 
                ?? throw new Exception("Failed to parse AI response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse travel plan response: {Response}", aiResponse);
            throw new Exception("Failed to parse travel plan response", ex);
        }
    }
}

public class HuggingFaceResponse
{
    [JsonPropertyName("generated_text")]
    public string GeneratedText { get; set; } = string.Empty;
}

public class TravelPlanResponse
{
    public string City { get; set; } = string.Empty;
    public List<Location> Locations { get; set; } = new();
    public string TotalDistance { get; set; } = string.Empty;
    public string TotalTime { get; set; } = string.Empty;
}

public class Location
{
    public string Name { get; set; } = string.Empty;
    public string DistanceFromPrevious { get; set; } = string.Empty;
    public string TimeToSpend { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
