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
        _apiKey = configuration["Gemini:ApiKey"] ?? throw new ArgumentNullException("Gemini:ApiKey is not configured");
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
        //return $@"Create a detailed one-day travel itinerary for the city of {city}.

        //    Return the response in valid JSON format using the following structure:
        //    {{
        //      ""city"": ""{city}"",
        //      ""locations"": [
        //        {{
        //          ""name"": ""Location Name"",
        //          ""distanceFromPrevious"": ""Distance in kilometers"",
        //          ""timeToSpend"": ""Time in hours"",
        //          ""description"": ""Brief description""
        //        }}
        //      ],
        //      ""totalDistance"": ""Total distance in kilometers"",
        //      ""totalTime"": ""Total time in hours""
        //    }}

        //    Constraints:
        //    - Only return the JSON.
        //    - Ensure the JSON is complete and well-formed.
        //    - Avoid special characters or non-English text in any of the fields.";

        return $@"Create a detailed one-day travel itinerary for the city of {city}.

            Determine the standard distance unit ('km' or 'miles') predominantly used in the country/region of {city}. Use this determined unit consistently for all distance values in the output.
            Calculate travel durations between locations and time spent at locations in hours and minutes, formatted precisely as 'Xh Ym' (e.g., ""1h 30m"", ""0h 45m"").

            Return the response *only* as a valid JSON object using the following structure. Do not include any text before or after the JSON.

            {{
              ""city"": ""{city}"",
              ""locations"": [
                {{
                  ""name"": ""Location Name"",
                  ""distanceFromPrevious"": ""Value Unit"", // e.g., ""2.5 km"" or ""1.5 miles"". Unit MUST be 'km' or 'miles', chosen based on city's region. For the first location, use ""0 Unit"" or distance from a logical start point.
                  ""timeToSpend"": ""Xh Ym"", // Estimated time spent *at* the location, e.g., ""2h 0m"", ""0h 45m"".
                  ""description"": ""Brief description of the location and activity.""
                }}
              ],
              ""totalDistance"": ""TotalValue Unit"", // Approximate total distance travelled between listed locations, using the determined unit (km or miles).
              ""totalTime"": ""Total Xh Ym"" // Estimated total duration for the day's itinerary, including travel and visit times.
            }}

            Constraints:
            - Replace {{city}} with the target city name.
            - The AI model MUST determine the appropriate distance unit ('km' or 'miles') based on the common standard for the {city}'s geographical location (country/region).
            - The chosen distance unit MUST be used consistently (either 'km' or 'miles') across all distance fields (`distanceFromPrevious`, `totalDistance`).
            - The output MUST be only the JSON object, starting with `{{` and ending with `}}`. No introductory text, explanations, or markdown formatting.
            - Ensure the JSON is complete, well-formed, and syntactically valid.
            - Use standard English characters only; avoid special symbols or non-English text.
            - Provide realistic estimates for distances between consecutive locations and the time needed at each location for a tourist.
            - Format all time duration strings precisely as 'Xh Ym'. Use '0h' for durations less than an hour (e.g., ""0h 45m"").
            - Format all distance strings precisely as 'Value Unit' where Value is a number (integer or decimal) and Unit is the determined distance unit ('km' or 'miles'). Example: ""5.2 km"", ""3 miles"".
            - The `distanceFromPrevious` for the first location should logically represent the start (e.g., ""0 km"", ""0 miles"", or distance from a central point using the determined unit).
            - `totalDistance` should represent the sum of distances travelled between the listed locations (effectively, the sum of `distanceFromPrevious` for all locations except potentially the first, depending on its definition), using the determined unit.
            - `totalTime` should represent a realistic estimate for the overall duration of the planned activities for the day, reasonably accounting for both the time spent at locations (`timeToSpend`) and estimated travel time between them.
            ";
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