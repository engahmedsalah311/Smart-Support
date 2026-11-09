using Microsoft.Identity.Client;
using SmartSupport.Api.AI.Abstraction;
using SmartSupport.Api.AI.Abstraction.DTOs;
using SmartSupport.Api.AI.Providers.Ollama.DTOs;
using SmartSupport.Api.Models.AI;
using System.Linq;

namespace SmartSupport.Api.Services.AI
{
    public class AiAssistantService : IAiService
    {
        private readonly ILLMClientResolver _llmClientResolver;
        private readonly IEmbeddingClient _embeddingClient;
        private readonly IVectorSearchService _vectorSearchService;

        public AiAssistantService(ILLMClientResolver llmClientResolver, IEmbeddingClient embeddingClient, IVectorSearchService vectorSearchService)
        {
            _llmClientResolver = llmClientResolver;
            _embeddingClient = embeddingClient;
            _vectorSearchService = vectorSearchService;
        }
        public async Task<ChatResponse> ChatAsync(string message, CancellationToken cancellationToken = default)
        {
            // 1. Convert user question into embedding
            var queryVector = await _embeddingClient.GenerateEmbeddingAsync(
                $"search_query: {message}",
                cancellationToken);

            // 2. Retrieve the most relevant chunks from Qdrant
            var retrievedChunks = await _vectorSearchService.SearchAsync(
                queryVector,
                limit: 5,
                cancellationToken);

            if (retrievedChunks.Count == 0)
            {
                return new ChatResponse
                {
                    Answer = "I don't have enough information in the available documents to answer this question.",
                    Sources = [],
                    Metrics = new ChatMetrics()
                };
            }

            // 3. Build context from retrieved chunks
            var context = string.Join(
                "\n\n",
                retrievedChunks.Select(chunk =>
                    $"Source: {chunk.FileName}, Page: {chunk.PageNumber}\n{chunk.Text}"));

            // Temporary: return the retrieved context through the LLM
            var prompt = $"""
                        You are SmartSupport, a customer support assistant.
                        Your task is to answer the user's question using ONLY the supplied context.

                        CRITICAL GROUNDING RULES:
                        1. Every factual statement in your answer MUST be directly supported by the context.
                        2. Do not use external knowledge, assumptions, guesses, or information from memory.
                        3. Never add steps, procedures, names, departments, URLs, numbers, or time periods
                           unless they are explicitly mentioned in the context.
                        4. Do not complete missing information using your own knowledge.
                        5. If the context does not contain the answer, say that the available documents
                           do not provide enough information.
                        6. If the context supports only part of the answer, provide only that part.
                        7. Do not mention actions that the user should take unless those actions
                           are explicitly described in the context.

                        ANSWERING PROCESS:
                        1. Identify the information in the context that directly answers the question.
                        2. Ignore unrelated information.
                        3. Build a short answer using only the identified information.
                        4. Before responding, verify that every sentence is supported by the context.
                        5. Remove any sentence that contains unsupported information.

                        LANGUAGE RULES:
                        1. Answer in the same language as the user's question.
                        2. If the question is in Arabic, write natural, clear Arabic.
                        3. Translate the meaning of the context into Arabic when necessary.
                        4. Do not mix Arabic and English unnecessarily.
                        5. Preserve product names, email subjects, and technical terms when needed.

                        RESPONSE STYLE:
                        - Be concise and direct.
                        - Prefer short paragraphs or numbered steps when appropriate.
                        - Do not repeat the same instruction.
                        - Do not add introductions or conclusions that do not help answer the question.

                        CONTEXT:
                        {context}

                        USER QUESTION:
                        {message}

                        ANSWER:
                        """;

            var LLMClient = _llmClientResolver.Resolve();
            var LLMResponse = await LLMClient.GenerateAsync(prompt, cancellationToken);
            var metrics = MapMetrics(LLMResponse.Metrics);

            return new ChatResponse
            {
                Answer = LLMResponse.Content,
                Metrics = MapMetrics(LLMResponse.Metrics),
                Sources = retrievedChunks.Select(chunk => new ChatSource
                {
                    FileName = chunk.FileName,
                    PageNumber = chunk.PageNumber,
                    ChunkIndex = chunk.ChunkIndex,
                    Score = chunk.Score
                }).ToList()
            };

        }

        private static ChatMetrics MapMetrics(LLMMetrics? metrics)
        {
            if (metrics is null)
                return new ChatMetrics();

            double? tokensPerSecond = null;

            if (metrics.GenerationDurationNanoseconds is > 0 &&
                metrics.OutputTokens.HasValue)
            {
                var durationInSeconds =
                    metrics.GenerationDurationNanoseconds.Value / 1_000_000_000d;

                tokensPerSecond =
                    metrics.OutputTokens.Value / durationInSeconds;
            }

            return new ChatMetrics
            {
                TotalDurationMs = metrics.TotalDurationNanoseconds.HasValue
                    ? metrics.TotalDurationNanoseconds.Value / 1_000_000d
                    : null,

                InputTokens = metrics.InputTokens,
                OutputTokens = metrics.OutputTokens,
                TotalTokens = metrics.TotalTokens,
                CachedInputTokens = metrics.CachedInputTokens,
                ReasoningTokens = metrics.ReasoningTokens,
                TokensPerSecond = tokensPerSecond
            };
        }
    }
}
