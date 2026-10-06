using FlowDesk.AI.Application.AI;

namespace FlowDesk.AI.Application.Abstractions.AI;

public interface IFlowDeskAiService
{
    Task<string> GetCapabilitiesAsync(CancellationToken cancellationToken = default);

    Task<AiAnswerDto> AskAsync(
        string question,
        int topK = 5,
        CancellationToken cancellationToken = default);
}
