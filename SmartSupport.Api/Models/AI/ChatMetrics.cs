namespace SmartSupport.Api.Models.AI
{
    public class ChatMetrics
    {
        public double? TotalDurationMs { get; set; }

        public long? InputTokens { get; set; }

        public long? OutputTokens { get; set; }

        public double? TokensPerSecond { get; set; }
        public long? TotalTokens { get; set; }

        public long? CachedInputTokens { get; set; }

        public long? ReasoningTokens { get; set; }


    }
}
