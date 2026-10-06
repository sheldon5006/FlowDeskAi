namespace FlowDesk.AI.Application.Knowledge;

public sealed record KnowledgeSearchResultDto(
    Guid ChunkId,
    Guid DocumentId,
    Guid BusinessId,
    string Source,
    int ChunkIndex,
    string Content,
    double Similarity);
