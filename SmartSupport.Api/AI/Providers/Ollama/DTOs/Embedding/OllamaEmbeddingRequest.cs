namespace SmartSupport.Api.AI.Providers.Ollama.DTOs.Embedding
{
    public class OllamaEmbeddingRequest
    {
        public string Model { get; set; } = string.Empty;
        public string Input { get; set; } = string.Empty;

    }
}
