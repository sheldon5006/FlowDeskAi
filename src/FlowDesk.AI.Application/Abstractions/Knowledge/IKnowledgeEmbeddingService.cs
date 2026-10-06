namespace FlowDesk.AI.Application.Abstractions.Knowledge;

public interface IKnowledgeEmbeddingService
{
    Task<int> EmbedDocumentAsync(
        Guid businessId,
        Guid documentId,
        CancellationToken cancellationToken = default);
}
