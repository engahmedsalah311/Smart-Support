using SmartSupport.Api.AI.Documents.Dtos;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace SmartSupport.Api.AI.Documents
{
    public class PdfTextExtractor
    {
        public List<ExtractedPdfPage> Extract(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(
                    "PDF file was not found.",
                    filePath);
            }

            var extractedPages = new List<ExtractedPdfPage>();

            using var document = PdfDocument.Open(filePath);

            foreach (var page in document.GetPages())
            {
                var text = ContentOrderTextExtractor.GetText(page);

                extractedPages.Add(new ExtractedPdfPage
                {
                    PageNumber = page.Number,
                    Text = text
                });
            }

            return extractedPages;
        }
    }
}
