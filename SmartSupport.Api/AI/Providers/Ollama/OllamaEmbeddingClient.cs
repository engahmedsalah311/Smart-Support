
using Microsoft.Extensions.Options;
using SmartSupport.Api.AI.Abstraction;
using SmartSupport.Api.AI.Providers.Ollama.DTOs.Embedding;
using SmartSupport.Api.Options.Ollama;
using System.Net.Http.Json;
namespace SmartSupport.Api.AI.Providers.Ollama
{
    public class OllamaEmbeddingClient : IEmbeddingClient
    {
        private readonly HttpClient _httpClient;
        private readonly OllamaOptions _options;

        public OllamaEmbeddingClient(HttpClient httpClient, IOptions<OllamaOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
        {
            var request = new OllamaEmbeddingRequest
            {
                Model = _options.EmbeddingModel,
                Input = text
            };

            var response = await _httpClient.PostAsJsonAsync(
                _options.EmbeddingUrl,
                request,
                cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.
                ReadFromJsonAsync<OllamaEmbeddingResponse>(cancellationToken: cancellationToken);

            return result?.Embeddings.FirstOrDefault()?.ToArray()
            ?? throw new InvalidOperationException(
                "Ollama returned an empty embedding.");

        }
    }
}
