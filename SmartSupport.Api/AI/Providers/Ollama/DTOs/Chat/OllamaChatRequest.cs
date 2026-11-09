using SmartSupport.Api.Options.Ollama;

namespace SmartSupport.Api.AI.Providers.Ollama.DTOs.Chat
{
    public class OllamaChatRequest
    {
        public string Model { get; set; } = string.Empty;

        public List<OllamaMessage> Messages { get; set; } = [];

        public bool Stream { get; set; }
        public OllamaGenerationOptions Options { get; set; } = new();
    }

    public class OllamaMessage
    {
        public string Role { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;
    }
}
