using Microsoft.Extensions.Options;
using SmartSupport.Api.AI.Abstraction;
using SmartSupport.Api.Options;

namespace SmartSupport.Api.Services
{
    public class LLMClientResolver : ILLMClientResolver
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly AIOptions _options;

        public LLMClientResolver(
            IServiceProvider serviceProvider,
            IOptions<AIOptions> options)
        {
            _serviceProvider = serviceProvider;
            _options = options.Value;
        }

        public ILLMClient Resolve()
        {
            return _options.LLMProvider.ToLowerInvariant() switch
            {
                "ollama" => _serviceProvider
                    .GetRequiredKeyedService<ILLMClient>("Ollama"),

                "openai" => _serviceProvider
                    .GetRequiredKeyedService<ILLMClient>("OpenAI"),

                _ => throw new InvalidOperationException(
                    $"Unsupported LLM Provider: {_options.LLMProvider}")
            };
        }
    }
}
