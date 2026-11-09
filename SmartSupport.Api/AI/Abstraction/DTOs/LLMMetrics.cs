namespace SmartSupport.Api.AI.Abstraction.DTOs
{
    public class LLMMetrics
    {
        public long? TotalDurationNanoseconds { get; set; }

        public long? InputTokens { get; set; }

        public long? OutputTokens { get; set; }

        public long? TotalTokens { get; set; }

        public long? CachedInputTokens { get; set; }

        public long? ReasoningTokens { get; set; }

        public long? GenerationDurationNanoseconds { get; set; }


    }
}
