using FlowDesk.AI.Application.Abstractions.Embeddings;
using FlowDesk.AI.Application.Abstractions.Knowledge;
using FlowDesk.AI.Application.Knowledge;
using FlowDesk.AI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Pgvector;

namespace FlowDesk.AI.Infrastructure.Knowledge;

public sealed class KnowledgeSearchService(
    FlowDeskDbContext dbContext,
    IEmbeddingProvider embeddingProvider) : IKnowledgeSearchService
{
    public async Task<IReadOnlyList<KnowledgeSearchResultDto>> SearchAsync(
        string query,
        int topK = 5,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentException("Query is required.", nameof(query));
        }

        topK = Math.Clamp(topK, 1, 20);

        var embeddings = await embeddingProvider.GenerateEmbeddingsAsync(
            [query.Trim()],
            cancellationToken);

        if (embeddings.Count != 1)
        {
            throw new InvalidOperationException(
                "The embedding provider did not return exactly one query vector.");
        }

        var queryVector = new Vector(embeddings[0]);

        return await dbContext.KnowledgeChunks
            .AsNoTracking()
            .Where(x => x.Embedding != null)
            .OrderBy(x => EF.Functions.CosineDistance(x.Embedding!, queryVector))
            .Take(topK)
            .Select(x => new KnowledgeSearchResultDto(
                x.Id,
                x.KnowledgeDocumentId,
                x.KnowledgeDocument.Source,
                x.ChunkIndex,
                x.Content,
                1d - EF.Functions.CosineDistance(x.Embedding!, queryVector)))
            .ToListAsync(cancellationToken);
    }
}
