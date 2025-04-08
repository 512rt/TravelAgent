using System.Text.Json.Serialization;
using TravelAgent.ServiceClients;

namespace TravelAgent.Models.Gemini
{
    public class GeminiCandidate
    {
        [JsonPropertyName("content")]
        public GeminiContent Content { get; set; }
    }
}
