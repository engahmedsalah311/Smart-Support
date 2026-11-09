namespace SmartSupport.Api.AI.Abstraction
{
    public interface IEmbeddingClient
    {
        Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default);
    }
}
