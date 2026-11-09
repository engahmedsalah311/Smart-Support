using SmartSupport.Api.AI.Abstraction.DTOs;

namespace SmartSupport.Api.AI.Abstraction
{
    public interface ILLMClient
    {
        Task<LLMResponse> GenerateAsync(
        string prompt,
        CancellationToken cancellationToken = default);

    }
}
