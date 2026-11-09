using SmartSupport.Api.AI.Documents.Enums;

namespace SmartSupport.Api.AI.Documents.Dtos
{
    public class DocumentUploadResult
    {
        public Guid Id { get; set; }

        public string FileName { get; set; } = string.Empty;

        public long FileSizeBytes { get; set; }

        public IngestionStatus Status { get; set; }

        public DateTime CreatedAtUtc { get; set; }

    }
}
