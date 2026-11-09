namespace SmartSupport.Api.Models.AI
{
    public class ChatResponse
    {
        public string Answer { get; set; } = string.Empty;

        public ChatMetrics Metrics { get; set; } = new();
        public List<ChatSource> Sources { get; set; } = [];

    }
}
