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
        Guid businessId,
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        if (businessId == Guid.Empty)
        {
            throw new ArgumentException("Business ID is required.", nameof(businessId));
        }

        if (documentId == Guid.Empty)
        {
            throw new ArgumentException("Document ID is required.", nameof(documentId));
        }

        var documentExists = await dbContext.KnowledgeDocuments
            .AnyAsync(
                x => x.Id == documentId && x.BusinessId == businessId,
                cancellationToken);

        if (!documentExists)
        {
            throw new ArgumentException(
                $"Document '{documentId}' was not found for the selected business.",
                nameof(documentId));
        }

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
