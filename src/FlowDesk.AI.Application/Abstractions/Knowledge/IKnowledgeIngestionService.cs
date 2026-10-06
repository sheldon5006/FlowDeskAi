using FlowDesk.AI.Application.Knowledge;

namespace FlowDesk.AI.Application.Abstractions.Knowledge;

public interface IKnowledgeIngestionService
{
    Task<KnowledgeIngestionResultDto> IngestAsync(
        Guid businessId,
        string source,
        string content,
        CancellationToken cancellationToken = default);

    Task<KnowledgeIngestionResultDto> IngestFileAsync(
        Guid businessId,
        string fileName,
        Stream fileStream,
        CancellationToken cancellationToken = default);
}
