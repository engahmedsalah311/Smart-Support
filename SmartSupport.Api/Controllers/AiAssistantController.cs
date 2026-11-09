using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartSupport.Api.AI.Abstraction;
using SmartSupport.Api.AI.Documents;
using SmartSupport.Api.AI.Tokenizer;
using SmartSupport.Api.Models.AI;
using SmartSupport.Api.Services.AI;

namespace SmartSupport.Api.Controllers
{
    [Route("api/ai")]
    [ApiController]
    public class AiAssistantController : ControllerBase
    {
        private readonly IAiService _aiService;
        

        public AiAssistantController(IAiService aiService)
        {
            _aiService = aiService;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat(
        [FromBody] ChatRequest request,
        CancellationToken cancellationToken)
        {
            var response = await _aiService.ChatAsync(
                request.Message,
                cancellationToken);

            return Ok(response);
        }

        [HttpPost("test-embedding")]
        
        public async Task<IActionResult> TestEmbedding(
        [FromBody] string text,
        [FromServices] IEmbeddingClient embeddingClient,
        CancellationToken cancellationToken)
        {
            var embedding = await embeddingClient.GenerateEmbeddingAsync(
                text,
                cancellationToken);

            return Ok(new
            {
                Dimensions = embedding.Length,
                Embedding = embedding
            });
        }

        [HttpPost("test-pdf-extraction")]
        public IActionResult TestPdfExtraction(
        [FromQuery] string filePath,
        [FromServices] PdfTextExtractor extractor)
        {
            var pages = extractor.Extract(filePath);

            return Ok(new
            {
                TotalPages = pages.Count,
                Pages = pages
            });
        }

        [HttpPost("test-pdf-chunking")]
        public IActionResult TestPdfChunking(
        [FromQuery] string filePath,
        [FromServices] PdfTextExtractor extractor,
        [FromServices] TextChunker chunker,
        [FromServices] TokenizerService tokenizer)
        {
            var pages = extractor.Extract(filePath);

            var chunks = chunker.ChunkPages(pages);

            return Ok(new
            {
                TotalPages = pages.Count,
                TotalChunks = chunks.Count,
                Chunks = chunks.Select(chunk => new
                {
                    chunk.ChunkIndex,
                    chunk.PageNumber,
                    CharacterCount = chunk.Text.Length,
                    TokenCount = tokenizer.CountTokens(chunk.Text),
                    chunk.Text
                })
            });
        }
        [HttpPost("test-token-count")]
        public IActionResult TestTokenCount(
        [FromBody] string text,
        [FromServices] TokenizerService tokenizer)
        {
            var tokenCount = tokenizer.CountTokens(text);

            return Ok(new
            {
                Text = text,
                TokenCount = tokenCount
            });
        }
    }
}
