namespace FlowDesk.AI.Application.Abstractions.Knowledge;

public interface IKnowledgeEmbeddingService
{
    Task<int> EmbedDocumentAsync(
        Guid documentId,
        CancellationToken cancellationToken = default);
}
