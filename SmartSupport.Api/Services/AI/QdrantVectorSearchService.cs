using Microsoft.Extensions.Options;
using Qdrant.Client;
using SmartSupport.Api.AI.Abstraction;
using SmartSupport.Api.AI.Documents.Dtos;
using SmartSupport.Api.Options;

namespace SmartSupport.Api.Services.AI
{
    public class QdrantVectorSearchService : IVectorSearchService
    {
        private readonly QdrantClient _qdrantClient;
        private readonly QdrantOptions _options;

        public QdrantVectorSearchService(QdrantClient qdrantClient, IOptions<QdrantOptions> options)
        {
            _qdrantClient = qdrantClient;
            _options = options.Value;
        }
        public async Task<List<RetrievedChunk>> SearchAsync(float[] queryVector, int limit = 5, CancellationToken cancellationToken = default)
        {
            if (queryVector == null || queryVector.Length != 768)
            {
                throw new ArgumentException("Query vector must contain 768 dimensions.",nameof(queryVector));
            }

            if (limit <= 0)
                throw new ArgumentOutOfRangeException(nameof(limit));

            var results = await _qdrantClient.QueryAsync(
                          collectionName: _options.CollectionName,
                          query: queryVector,
                          limit: (ulong)limit,
                          payloadSelector: true,
                          cancellationToken: cancellationToken);

            var chunks = new List<RetrievedChunk>();

            foreach (var result in results)
            {
                var payload = result.Payload;

                if(result.Score < 0.20f) continue; 
                chunks.Add(new RetrievedChunk
                {
                    DocumentId = Guid.Parse(
                        payload["document_id"].StringValue),

                    FileName = payload["file_name"].StringValue,

                    ChunkIndex = (int)payload["chunk_index"].IntegerValue,

                    PageNumber = (int)payload["page_number"].IntegerValue,

                    Text = payload["text"].StringValue,

                    Score = result.Score
                });
            }

            return chunks;
        }
    }
}
