using FlowDesk.AI.Application.Knowledge;

namespace FlowDesk.AI.Application.Abstractions.Knowledge;

public interface IKnowledgeService
{
    Task<KnowledgeDocumentDto> AddDocumentAsync(
        string source,
        string content,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<KnowledgeDocumentDto>> GetDocumentsAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<KnowledgeChunkDto>> GetChunksAsync(
        Guid documentId,
        CancellationToken cancellationToken = default);
}
