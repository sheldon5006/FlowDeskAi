namespace FlowDesk.AI.Application.Knowledge;

public sealed record KnowledgeChunkDto(
    Guid Id,
    Guid KnowledgeDocumentId,
    int ChunkIndex,
    string Content,
    DateTimeOffset CreatedAtUtc);
