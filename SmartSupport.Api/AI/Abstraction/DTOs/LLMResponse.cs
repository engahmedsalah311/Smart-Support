namespace SmartSupport.Api.AI.Abstraction.DTOs
{
    public class LLMResponse
    {
        public string Content { get; set; } = string.Empty;

        public LLMMetrics? Metrics { get; set; }

    }
}
