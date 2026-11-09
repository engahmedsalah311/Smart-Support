using System.Text.Json.Serialization;

namespace SmartSupport.Api.AI.Providers.OpenAI.DTOs
{
    public class OpenAIResponse
    {
        public string? Id { get; set; }

        public string? Status { get; set; }

        public List<OpenAIOutputItem> Output { get; set; } = [];

        public OpenAIUsage? Usage { get; set; }

        public OpenAIResponseError? Error { get; set; }

        [JsonPropertyName("incomplete_details")]
        public OpenAIIncompleteDetails? IncompleteDetails { get; set; }
    }

    public class OpenAIOutputItem
    {
        public string? Type { get; set; }

        public string? Role { get; set; }

        public List<OpenAIContentItem> Content { get; set; } = [];
    }

    public class OpenAIContentItem
    {
        public string? Type { get; set; }

        public string? Text { get; set; }

        public string? Refusal { get; set; }
    }

    public class OpenAIUsage
    {
        [JsonPropertyName("input_tokens")]
        public long? InputTokens { get; set; }

        [JsonPropertyName("output_tokens")]
        public long? OutputTokens { get; set; }

        [JsonPropertyName("total_tokens")]
        public long? TotalTokens { get; set; }

        [JsonPropertyName("input_tokens_details")]
        public OpenAIInputTokensDetails? InputTokensDetails { get; set; }

        [JsonPropertyName("output_tokens_details")]
        public OpenAIOutputTokensDetails? OutputTokensDetails { get; set; }
    }

    public class OpenAIInputTokensDetails
    {
        [JsonPropertyName("cached_tokens")]
        public long? CachedTokens { get; set; }
    }

    public class OpenAIOutputTokensDetails
    {
        [JsonPropertyName("reasoning_tokens")]
        public long? ReasoningTokens { get; set; }
    }
    public class OpenAIResponseError
    {
        public string? Code { get; set; }

        public string? Message { get; set; }
    }

    public class OpenAIIncompleteDetails
    {
        public string? Reason { get; set; }
    }
}

