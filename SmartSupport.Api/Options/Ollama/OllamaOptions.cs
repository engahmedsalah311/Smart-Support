namespace SmartSupport.Api.Options.Ollama
{
    public class OllamaOptions
    {
        public const string SectionName = "Ollama";

        public string BaseUrl { get; set; } = string.Empty;

        public string Model { get; set; } = string.Empty;

        public string ChatUrl => $"{BaseUrl.TrimEnd('/')}/api/chat";

        public string EmbeddingModel { get; set; } = string.Empty;
        public string EmbeddingUrl => $"{BaseUrl.TrimEnd('/')}/api/embed";


    }
}
