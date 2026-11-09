using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using SmartSupport.Api.AI.Abstraction;
using SmartSupport.Api.AI.Documents.Enums;
using SmartSupport.Api.Data;
using SmartSupport.Api.Options;
using System.Security.Cryptography;
using System.Text;

namespace SmartSupport.Api.AI.Documents
{
    public class DocumentProcessingService : IDocumentProcessingService
    {
        private readonly SmartSupportDbContext _dbContext;
        private readonly PdfTextExtractor _pdfTextExtractor;
        private readonly TextChunker _textChunker;
        private readonly IEmbeddingClient _embeddingClient;
        private readonly QdrantClient _qdrantClient;
        private readonly QdrantOptions _qdrantOptions;
        private readonly StorageOptions _storageOptions;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<DocumentProcessingService> _logger;

        public DocumentProcessingService(
            SmartSupportDbContext dbContext,
            PdfTextExtractor pdfTextExtractor,
            TextChunker textChunker,
            IEmbeddingClient embeddingClient,
            QdrantClient qdrantClient,
            IOptions<QdrantOptions> qdrantOptions,
            IOptions<StorageOptions> storageOptions,
            IWebHostEnvironment environment,
            ILogger<DocumentProcessingService> logger)
        {
            _dbContext = dbContext;
            _pdfTextExtractor = pdfTextExtractor;
            _textChunker = textChunker;
            _embeddingClient = embeddingClient;
            _qdrantClient = qdrantClient;
            _qdrantOptions = qdrantOptions.Value;
            _storageOptions = storageOptions.Value;
            _environment = environment;
            _logger = logger;
        }

        public async Task ProcessAsync(Guid documentId, CancellationToken cancellationToken = default)
        {
            var document = await _dbContext.Documents.FirstOrDefaultAsync(d => d.Id == documentId,cancellationToken);

            if (document is null)
                throw new InvalidOperationException($"Document {documentId} was not found.");

            try
            {
                document.Status = IngestionStatus.Processing;
                document.ErrorMessage = null;

                await _dbContext.SaveChangesAsync(cancellationToken);

                // 1. Locate the stored PDF.
                var filePath = Path.Combine(
                    _environment.ContentRootPath,
                    _storageOptions.DocumentsPath,
                    document.StoredFileName);

                // 2. Extract text from PDF.
                var pages = _pdfTextExtractor.Extract(filePath);

                // 3. Split extracted text into chunks.
                var chunks = _textChunker.ChunkPages(pages);

                if (chunks.Count == 0)
                    throw new InvalidOperationException(
                        "No text chunks were generated from the PDF.");

                // 4. Generate embeddings and prepare Qdrant points.
                var points = new List<PointStruct>();

                foreach (var chunk in chunks)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var embedding = await _embeddingClient
                        .GenerateEmbeddingAsync(
                            $"search_document: {chunk.Text}",
                            cancellationToken);

                    points.Add(new PointStruct
                    {
                        Id = CreatePointId(document.Id, chunk.ChunkIndex),
                        Vectors = embedding,
                        Payload =
                    {
                        ["document_id"] = document.Id.ToString(),
                        ["file_name"] = document.FileName,
                        ["chunk_index"] = chunk.ChunkIndex,
                        ["page_number"] = chunk.PageNumber,
                        ["text"] = chunk.Text
                    }
                    });
                }

                // 5. Store vectors and payloads in Qdrant.
                await _qdrantClient.UpsertAsync(
                    _qdrantOptions.CollectionName,
                    points);

                // 6. Mark document as completed.
                document.TotalChunks = chunks.Count;
                document.Status = IngestionStatus.Completed;
                document.ProcessedAtUtc = DateTime.UtcNow;

                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Document {DocumentId} processed successfully. Chunks: {ChunkCount}",
                    documentId,
                    chunks.Count);
            }
            catch (Exception ex)
            {
                document.Status = IngestionStatus.Failed;
                document.ErrorMessage = ex.Message;

                await _dbContext.SaveChangesAsync(
                    CancellationToken.None);

                _logger.LogError(
                    ex,
                    "Failed to process document {DocumentId}",
                    documentId);

                throw;
            }
        }

        private static Guid CreatePointId(
            Guid documentId,
            int chunkIndex)
        {
            var input = Encoding.UTF8.GetBytes(
                $"{documentId:N}:{chunkIndex}");

            var hash = SHA256.HashData(input);

            return new Guid(hash.AsSpan(0, 16));
        }
    }
}
