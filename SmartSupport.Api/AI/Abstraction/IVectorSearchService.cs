using SmartSupport.Api.AI.Documents.Dtos;

namespace SmartSupport.Api.AI.Abstraction
{
    public interface IVectorSearchService
    {
        Task<List<RetrievedChunk>> SearchAsync(float[] queryVector,int limit = 5,CancellationToken cancellationToken = default);
    }
}
