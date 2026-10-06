namespace FlowDesk.AI.Application.Knowledge;

public sealed record KnowledgeSearchResultDto(
    Guid ChunkId,
    Guid DocumentId,
    string Source,
    int ChunkIndex,
    string Content,
    double Similarity);
