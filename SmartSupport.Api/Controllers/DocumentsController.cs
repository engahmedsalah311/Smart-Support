using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartSupport.Api.AI.Documents;

namespace SmartSupport.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        private readonly DocumentIngestionService _ingestionService;

        public DocumentsController(DocumentIngestionService ingestionService)
        {
            _ingestionService = ingestionService;
        }


        [HttpPost]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(21 * 1024 * 1024)]
        public async Task<IActionResult> Upload(IFormFile file,CancellationToken cancellationToken)
        {
            try
            {
                var result = await _ingestionService.UploadAsync(
                    file,
                    cancellationToken);

                return Accepted(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{id:guid}/reprocess")]
        public async Task<IActionResult> Reprocess(
            Guid id,
            CancellationToken cancellationToken)
        {
            var queued = await _ingestionService.ReprocessAsync(
                id,
                cancellationToken);

            if (!queued)
            {
                return NotFound(new
                {
                    message = "Document was not found."
                });
            }

            return Accepted(new
            {
                documentId = id,
                message = "Document reprocessing has been queued."
            });
        }

    }
}
