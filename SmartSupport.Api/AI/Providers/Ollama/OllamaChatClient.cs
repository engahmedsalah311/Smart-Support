using Microsoft.Extensions.Options;
using SmartSupport.Api.AI.Abstraction;
using SmartSupport.Api.AI.Abstraction.DTOs;
using SmartSupport.Api.AI.Providers.Ollama.DTOs.Chat;
using SmartSupport.Api.Options.Ollama;

namespace SmartSupport.Api.AI.Providers.Ollama
{
    public class OllamaChatClient : ILLMClient
    {
        private readonly HttpClient _httpClient;
        private readonly OllamaOptions _options;

        public OllamaChatClient(
            HttpClient httpClient,
            IOptions<OllamaOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }
        

        public async Task<LLMResponse> GenerateAsync(string prompt, CancellationToken cancellationToken = default)
        {
            var request = new OllamaChatRequest
            {
                Model = _options.Model,
                Messages = new List<OllamaMessage>
                {
                    new OllamaMessage
                    {
                        Role = "user",
                        Content = prompt
                    }
                },
                Stream = false
            };

            using var response = await _httpClient.PostAsJsonAsync(_options.ChatUrl, request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var ollamaResponse = await response.Content
            .ReadFromJsonAsync<OllamaChatResponse>(cancellationToken: cancellationToken);

            if (ollamaResponse?.Message is null)
            {
                throw new InvalidOperationException(
                    "Ollama returned an invalid chat response.");
            }

            return new LLMResponse
            {
                Content = ollamaResponse.Message.Content,
                Metrics = new LLMMetrics
                {
                    TotalDurationNanoseconds = ollamaResponse.TotalDuration,

                    InputTokens = ollamaResponse.PromptEvalCount,
                    OutputTokens = ollamaResponse.EvalCount,

                    TotalTokens =
                        (ollamaResponse.PromptEvalCount ?? 0) +
                        (ollamaResponse.EvalCount ?? 0),

                    GenerationDurationNanoseconds = ollamaResponse.EvalDuration
                }
            };

        }
    }
}
