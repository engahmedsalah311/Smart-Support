namespace SmartSupport.Api.Models.AI
{
    public class ChatSource
    {
        public string FileName { get; set; } = string.Empty;

        public int PageNumber { get; set; }

        public int ChunkIndex { get; set; }

        public float Score { get; set; }

    }
}
