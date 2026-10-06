using FlowDesk.AI.Application.Knowledge;

namespace FlowDesk.AI.Application.Abstractions.Knowledge;

public interface IKnowledgeService
{
    Task<KnowledgeDocumentDto> AddDocumentAsync(
        Guid businessId,
        string source,
        string content,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<KnowledgeDocumentDto>> GetDocumentsAsync(
        Guid businessId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<KnowledgeChunkDto>> GetChunksAsync(
        Guid businessId,
        Guid documentId,
        CancellationToken cancellationToken = default);
}
