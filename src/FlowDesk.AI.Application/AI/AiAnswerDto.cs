using FlowDesk.AI.Application.Knowledge;

namespace FlowDesk.AI.Application.AI;

public sealed record AiAnswerDto(
    string Answer,
    IReadOnlyList<KnowledgeSearchResultDto> Sources);
