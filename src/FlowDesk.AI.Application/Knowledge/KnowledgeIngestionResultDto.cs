namespace FlowDesk.AI.Application.Knowledge;

public sealed record KnowledgeIngestionResultDto(
    KnowledgeDocumentDto Document,
    int EmbeddedChunkCount);
