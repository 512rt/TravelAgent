using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using TravelAgent.Models;
using TravelAgent.Models.Gemini;

namespace TravelAgent.ServiceClients;

public class TravelAiClient : ITravelAiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<TravelAiClient> _logger;
    private const string GeminiEndpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";

    public TravelAiClient(IConfiguration configuration, ILogger<TravelAiClient> logger)
    {
        _httpClient = new HttpClient();
        _apiKey = configuration["GeminiApiKey"] ?? throw new ArgumentNullException("GeminiApiKey is not configured");
        _logger = logger;
    }

    public async Task<TravelPlanResponse> GetTravelPlanAsync(string city)
    {
        try
        {
            _logger.LogInformation("Generating travel plan for city: {City}", city);

            var prompt = GenerateTravelPrompt(city);

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var requestUrl = $"{GeminiEndpoint}?key={_apiKey}";

            _logger.LogDebug("Sending request to Gemini API");
            var response = await _httpClient.PostAsync(requestUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Gemini API returned error: {StatusCode} - {Content}",
                    response.StatusCode, errorContent);
                throw new Exception($"Gemini API error: {response.StatusCode} - {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Received response from Gemini API: {Response}", responseContent);

            var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseContent);
            var text = geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

            if (string.IsNullOrWhiteSpace(text))
                throw new Exception("No content returned from Gemini API");

            return ParseTravelPlanResponse(text);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error generating travel plan for {city}");
            throw;
        }
    }

    private string GenerateTravelPrompt(string city)
    {
        return $@"Create a detailed one-day travel itinerary for the city of {city}.

            Return the response in valid JSON format using the following structure:
            {{
              ""city"": ""{city}"",
              ""locations"": [
                {{
                  ""name"": ""Location Name"",
                  ""distanceFromPrevious"": ""Distance in kilometers"",
                  ""timeToSpend"": ""Time in hours"",
                  ""description"": ""Brief description""
                }}
              ],
              ""totalDistance"": ""Total distance in kilometers"",
              ""totalTime"": ""Total time in hours""
            }}
            
            Constraints:
            - Only return the JSON.
            - Ensure the JSON is complete and well-formed.
            - Avoid special characters or non-English text in any of the fields.";
    }

    private TravelPlanResponse ParseTravelPlanResponse(string aiResponse)
    {
        try
        {
            _logger.LogDebug("Parsing AI response: {Response}", aiResponse);

            var jsonStart = aiResponse.IndexOf('{');
            var jsonEnd = aiResponse.LastIndexOf('}');
            if (jsonStart == -1 || jsonEnd == -1 || jsonEnd <= jsonStart)
                throw new Exception("Could not extract JSON from AI response.");

            var rawJson = aiResponse.Substring(jsonStart, jsonEnd - jsonStart + 1);
            var cleanedJson = rawJson
                .Replace("\r", "")
                .Replace("\n", "")
                .Replace("\t", "")
                .Replace(",}", "}")
                .Replace(",]", "]");

            _logger.LogDebug("Cleaned JSON content: {Json}", cleanedJson);
            var travelPlan = Newtonsoft.Json.JsonConvert.DeserializeObject<TravelPlanResponse>(cleanedJson);

            return travelPlan ?? throw new Exception("Deserialized object is null");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse travel plan response: {Response}", aiResponse);
            throw new Exception("Failed to parse travel plan response", ex);
        }
    }
}