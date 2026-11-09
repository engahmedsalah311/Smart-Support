namespace SmartSupport.Api.AI.Providers.Ollama.DTOs.Embedding
{
    public class OllamaEmbeddingResponse
    {
        public string Model { get; set; } = string.Empty;
        public List<List<float>> Embeddings { get; set; } = [];
        public long TotalDuration { get; set; }
        public long LoadDuration { get; set; }
        public int PromptEvalCount { get; set; }
    }
}
