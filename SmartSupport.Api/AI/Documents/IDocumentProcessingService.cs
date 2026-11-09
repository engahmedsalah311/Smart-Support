namespace SmartSupport.Api.AI.Documents
{
    public interface IDocumentProcessingService
    {
        Task ProcessAsync(Guid documentId, CancellationToken cancellationToken = default);
    }
}
