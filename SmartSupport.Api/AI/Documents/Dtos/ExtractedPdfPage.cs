namespace SmartSupport.Api.AI.Documents.Dtos
{
    public class ExtractedPdfPage
    {
        public int PageNumber { get; set; }

        public string Text { get; set; } = string.Empty;
    }
}
