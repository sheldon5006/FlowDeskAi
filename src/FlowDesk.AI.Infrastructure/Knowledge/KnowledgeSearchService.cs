using FlowDesk.AI.Application.Abstractions.Embeddings;
using FlowDesk.AI.Application.Abstractions.Knowledge;
using FlowDesk.AI.Application.Knowledge;
using FlowDesk.AI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace FlowDesk.AI.Infrastructure.Knowledge;

public sealed class KnowledgeSearchService(
    FlowDeskDbContext dbContext,
    IEmbeddingProvider embeddingProvider,
    IConfiguration configuration) : IKnowledgeSearchService
{
    public async Task<IReadOnlyList<KnowledgeSearchResultDto>> SearchAsync(
        Guid businessId,
        string query,
        int topK = 5,
        CancellationToken cancellationToken = default)
    {
        if (businessId == Guid.Empty)
        {
            throw new ArgumentException("Business ID is required.", nameof(businessId));
        }

        if (string.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentException("Query is required.", nameof(query));
        }

        topK = Math.Clamp(topK, 1, 20);

        var minimumSimilarity = double.TryParse(
            configuration["RAG_MIN_SIMILARITY"],
            out var configuredThreshold)
            ? configuredThreshold
            : 0.25;

        minimumSimilarity = Math.Clamp(minimumSimilarity, 0d, 1d);

        var embeddings = await embeddingProvider.GenerateEmbeddingsAsync(
            [query.Trim()],
            cancellationToken);

        if (embeddings.Count != 1)
        {
            throw new InvalidOperationException(
                "The embedding provider did not return exactly one query vector.");
        }

        var queryVector = new Vector(embeddings[0]);
        var maximumDistance = 1d - minimumSimilarity;

        var matches = await dbContext.KnowledgeChunks
            .AsNoTracking()
            .Where(x =>
                x.Embedding != null &&
                x.KnowledgeDocument.BusinessId == businessId &&
                x.Embedding!.CosineDistance(queryVector) <= maximumDistance)
            .OrderBy(x => x.Embedding!.CosineDistance(queryVector))
            .Take(topK)
            .Select(x => new
            {
                x.Id,
                x.KnowledgeDocumentId,
                BusinessId = x.KnowledgeDocument.BusinessId,
                Source = x.KnowledgeDocument.Source,
                x.ChunkIndex,
                x.Content,
                Distance = x.Embedding!.CosineDistance(queryVector)
            })
            .ToListAsync(cancellationToken);

        return matches
            .Select(x => new KnowledgeSearchResultDto(
                x.Id,
                x.KnowledgeDocumentId,
                x.BusinessId,
                x.Source,
                x.ChunkIndex,
                x.Content,
                1d - x.Distance))
            .ToList();
    }
}
