using FlowDesk.AI.Application.Abstractions.Embeddings;
using FlowDesk.AI.Application.Abstractions.Knowledge;
using FlowDesk.AI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Pgvector;

namespace FlowDesk.AI.Infrastructure.Knowledge;

public sealed class KnowledgeEmbeddingService(
    FlowDeskDbContext dbContext,
    IEmbeddingProvider embeddingProvider) : IKnowledgeEmbeddingService
{
    public async Task<int> EmbedDocumentAsync(
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        var chunks = await dbContext.KnowledgeChunks
            .Where(x => x.KnowledgeDocumentId == documentId && x.Embedding == null)
            .OrderBy(x => x.ChunkIndex)
            .ToListAsync(cancellationToken);

        if (chunks.Count == 0)
        {
            return 0;
        }

        var embeddings = await embeddingProvider.GenerateEmbeddingsAsync(
            chunks.Select(x => x.Content).ToArray(),
            cancellationToken);

        if (embeddings.Count != chunks.Count)
        {
            throw new InvalidOperationException(
                "The embedding provider returned a different number of vectors than chunks.");
        }

        for (var i = 0; i < chunks.Count; i++)
        {
            chunks[i].Embedding = new Vector(embeddings[i]);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return chunks.Count;
    }
}
