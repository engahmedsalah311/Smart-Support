namespace SmartSupport.Api.AI.Documents.Dtos
{
    public class RetrievedChunk
    {
        public Guid DocumentId { get; set; }

        public string FileName { get; set; } = string.Empty;

        public int ChunkIndex { get; set; }

        public int PageNumber { get; set; }

        public string Text { get; set; } = string.Empty;

        public float Score { get; set; }
    }
}
