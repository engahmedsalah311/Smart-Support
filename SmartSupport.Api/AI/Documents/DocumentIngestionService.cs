using Hangfire;
using Microsoft.EntityFrameworkCore;
using SmartSupport.Api.AI.Documents.Dtos;
using SmartSupport.Api.AI.Documents.Entities;
using SmartSupport.Api.AI.Documents.Enums;
using SmartSupport.Api.BackgroundJobs;
using SmartSupport.Api.Data;
using SmartSupport.Api.Services.FileStorage;
using UglyToad.PdfPig;

namespace SmartSupport.Api.AI.Documents
{
    public class DocumentIngestionService
    {
        private const long MaxFileSizeBytes = 20 * 1024 * 1024;

        private readonly SmartSupportDbContext _dbContext;
        private readonly IFileStorageService _fileStorageService;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public DocumentIngestionService(SmartSupportDbContext dbContext,IFileStorageService fileStorageService,IBackgroundJobClient backgroundJobClient)
        {
            _dbContext = dbContext;
            _fileStorageService = fileStorageService;
            _backgroundJobClient = backgroundJobClient;
        }

        public async Task<DocumentUploadResult> UploadAsync(
            IFormFile file,
            CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Please select a valid PDF file.");

            if (!string.Equals(Path.GetExtension(file.FileName),".pdf",StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Only PDF files are allowed.");
            }

            if (file.Length > MaxFileSizeBytes)
                throw new ArgumentException("Maximum file size is 20 MB.");

            // Validate the PDF signature and structure.
            int totalPages;

            await using (var validationStream = file.OpenReadStream())
            {
                var signature = new byte[5];//%PDF-

                var bytesRead = await validationStream.ReadAsync(signature.AsMemory(),cancellationToken);

                if (bytesRead != 5 || !signature.SequenceEqual("%PDF-"u8.ToArray()))
                {
                    throw new ArgumentException("Invalid PDF file.");
                }

                try
                {
                    using var pdfDocument = PdfDocument.Open(validationStream);

                    totalPages = pdfDocument.GetPages().Count();

                    if (totalPages == 0)
                        throw new ArgumentException("PDF contains no pages.");
                }
                catch (ArgumentException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new ArgumentException("Invalid or corrupted PDF file.", ex);
                }
            }

            var storedFileName = $"{Guid.NewGuid():N}.pdf";

            var document = new Document
            {
                Id = Guid.NewGuid(),
                FileName = Path.GetFileName(file.FileName),
                StoredFileName = storedFileName,
                ContentType = "application/pdf",
                FileSizeBytes = file.Length,
                TotalPages = totalPages,
                Status = IngestionStatus.Pending,
                CreatedAtUtc = DateTime.UtcNow
            };

            // Save PDF to local storage.
            await using (var content = file.OpenReadStream())
            {
                document.FilePath = await _fileStorageService.SaveAsync(
                    content,
                    storedFileName,
                    cancellationToken);
            }

            try
            {
                _dbContext.Documents.Add(document);

                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                try
                {
                    await _fileStorageService.DeleteAsync(storedFileName);
                }
                catch
                {
                    // Preserve the original database exception.
                    // Cleanup failure should be logged in production.
                }

                throw;
            }

            _backgroundJobClient.Enqueue<DocumentProcessingJob>(
                job => job.ExecuteAsync(document.Id));

            return new DocumentUploadResult
            {
                Id = document.Id,
                FileName = document.FileName,
                FileSizeBytes = document.FileSizeBytes,
                Status = document.Status,
                CreatedAtUtc = document.CreatedAtUtc
            };
        }

        public async Task<bool> ReprocessAsync(
            Guid documentId,
            CancellationToken cancellationToken = default)
        {
            var document = await _dbContext.Documents
                .FirstOrDefaultAsync(
                    d => d.Id == documentId,
                    cancellationToken);

            if (document is null)
                return false;

            _backgroundJobClient.Enqueue<DocumentProcessingJob>(
                job => job.ExecuteAsync(documentId));

            return true;
        }
    }
}
