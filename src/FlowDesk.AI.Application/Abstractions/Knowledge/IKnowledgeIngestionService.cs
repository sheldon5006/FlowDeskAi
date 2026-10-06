using FlowDesk.AI.Application.Knowledge;

namespace FlowDesk.AI.Application.Abstractions.Knowledge;

public interface IKnowledgeIngestionService
{
    Task<KnowledgeIngestionResultDto> IngestAsync(
        string source,
        string content,
        CancellationToken cancellationToken = default);
}
