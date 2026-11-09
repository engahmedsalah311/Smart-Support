using SmartSupport.Api.AI.Providers.Ollama.DTOs;
using SmartSupport.Api.Models.AI;

namespace SmartSupport.Api.Services.AI
{
    public interface IAiService
    {
        Task<ChatResponse> ChatAsync(string message, CancellationToken cancellationToken = default);
    }
}
