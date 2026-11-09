using System.Text.Json.Serialization;

namespace SmartSupport.Api.Options.Ollama
{
    public class OllamaGenerationOptions
    {
        [JsonPropertyName("num_predict")]
        public int NumPredict { get; set; } = 256;
    }
}
