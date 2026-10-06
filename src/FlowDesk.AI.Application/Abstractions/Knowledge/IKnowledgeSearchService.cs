using FlowDesk.AI.Application.Knowledge;

namespace FlowDesk.AI.Application.Abstractions.Knowledge;

public interface IKnowledgeSearchService
{
    Task<IReadOnlyList<KnowledgeSearchResultDto>> SearchAsync(
        string query,
        int topK = 5,
        CancellationToken cancellationToken = default);
}
