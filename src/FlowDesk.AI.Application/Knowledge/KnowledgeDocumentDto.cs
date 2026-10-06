namespace FlowDesk.AI.Application.Knowledge;

public sealed record KnowledgeDocumentDto(
    Guid Id,
    string Source,
    string Content,
    int ChunkCount,
    DateTimeOffset CreatedAtUtc);
