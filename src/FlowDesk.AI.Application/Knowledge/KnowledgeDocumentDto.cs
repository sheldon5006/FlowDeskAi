namespace FlowDesk.AI.Application.Knowledge;

public sealed record KnowledgeDocumentDto(
    Guid Id,
    Guid BusinessId,
    string Source,
    string Content,
    int ChunkCount,
    DateTimeOffset CreatedAtUtc);
