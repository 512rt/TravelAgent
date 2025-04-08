using System.Text.Json.Serialization;
using TravelAgent.ServiceClients;

namespace TravelAgent.Models.Gemini
{
    public class GeminiContent
    {
        [JsonPropertyName("parts")]
        public List<GeminiPart> Parts { get; set; }
    }
}
