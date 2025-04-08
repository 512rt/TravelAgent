using System.Text.Json.Serialization;

namespace TravelAgent.Models.Gemini
{
    public class GeminiPart
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }
    }
}
