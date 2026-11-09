using SmartSupport.Api.AI.Documents.Enums;

namespace SmartSupport.Api.AI.Documents.Entities
{
    public class Document
    {
        public Guid Id { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string StoredFileName { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;

        public long FileSizeBytes { get; set; }

        public int TotalPages { get; set; }

        public int TotalChunks { get; set; }

        public IngestionStatus Status { get; set; }

        public string? ErrorMessage { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public DateTime? ProcessedAtUtc { get; set; }
    }
}
