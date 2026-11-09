using Microsoft.Extensions.Options;
using SmartSupport.Api.AI.Abstraction;
using SmartSupport.Api.AI.Abstraction.DTOs;
using SmartSupport.Api.AI.Providers.OpenAI.DTOs;
using SmartSupport.Api.Options.OpenAI;
using System.Diagnostics;
using System.Text.Json;

namespace SmartSupport.Api.AI.Providers.OpenAI
{
    public class OpenAIChatClient:ILLMClient
    {
        private readonly HttpClient _httpClient;
        private readonly OpenAIOptions _options;

        public OpenAIChatClient(
            HttpClient httpClient,
            IOptions<OpenAIOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<LLMResponse> GenerateAsync(string prompt, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();

            var request = new OpenAIRequest
            {
                Model = _options.Model,
                Input = prompt
            };

            using var response = await _httpClient.PostAsJsonAsync(
                "responses",
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(
                    cancellationToken);

                throw new HttpRequestException(
                    $"OpenAI API returned {(int)response.StatusCode}: {errorBody}",
                    null,
                    response.StatusCode);
            }

            var openAIResponse = await response.Content
                .ReadFromJsonAsync<OpenAIResponse>(
                    cancellationToken: cancellationToken);

            if (openAIResponse is null)
            {
                throw new InvalidOperationException(
                    "OpenAI returned an invalid response.");
            }

            // Validate response status
            switch (openAIResponse.Status)
            {
                case "completed":
                    break;

                case "incomplete":
                    throw new InvalidOperationException(
                        $"OpenAI response is incomplete. Reason: " +
                        $"{openAIResponse.IncompleteDetails?.Reason ?? "Unknown"}");

                case "failed":
                    throw new InvalidOperationException(
                        $"OpenAI response failed. Error: " +
                        $"{openAIResponse.Error?.Message ?? "Unknown error"}");

                default:
                    throw new InvalidOperationException(
                        $"Unexpected OpenAI response status: " +
                        $"{openAIResponse.Status ?? "null"}");
            }

            // Extract generated text and refusal
            var answer = string.Concat(
                openAIResponse.Output
                    .Where(item => item.Type == "message")
                    .SelectMany(item => item.Content)
                    .Where(content =>
                        content.Type == "output_text" ||
                        content.Type == "refusal")
                    .Select(content =>
                        content.Type == "refusal"
                            ? content.Refusal
                            : content.Text));

            if (string.IsNullOrWhiteSpace(answer))
            {
                throw new InvalidOperationException(
                    "OpenAI returned an empty response.");
            }

            stopwatch.Stop();

            var durationNanoseconds =
                (long)(stopwatch.Elapsed.TotalMilliseconds * 1_000_000d);

            var usage = openAIResponse.Usage;

            return new LLMResponse
            {
                Content = answer,

                Metrics = new LLMMetrics
                {
                    TotalDurationNanoseconds = durationNanoseconds,

                    InputTokens = usage?.InputTokens,
                    OutputTokens = usage?.OutputTokens,
                    TotalTokens = usage?.TotalTokens,

                    CachedInputTokens = usage?.InputTokensDetails?.CachedTokens,
                    ReasoningTokens = usage?.OutputTokensDetails?.ReasoningTokens
                }
            };
        }
    }
}
