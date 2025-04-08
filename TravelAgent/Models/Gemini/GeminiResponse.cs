using System.Text.Json.Serialization;
using TravelAgent.ServiceClients;

namespace TravelAgent.Models.Gemini
{
    public class GeminiResponse
    {
        [JsonPropertyName("candidates")]
        public List<GeminiCandidate> Candidates { get; set; }
    }
}
