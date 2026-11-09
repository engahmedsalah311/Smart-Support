using SmartSupport.Api.AI.Documents;

namespace SmartSupport.Api.BackgroundJobs
{
    public class DocumentProcessingJob
    {
        private readonly IDocumentProcessingService _processingService;

        public DocumentProcessingJob(
            IDocumentProcessingService processingService)
        {
            _processingService = processingService;
        }

        public Task ExecuteAsync(Guid documentId)
        {
            return _processingService.ProcessAsync(documentId);
        }
    }
}
