namespace FlowDesk.AI.Application.Abstractions.AI;

public interface IFlowDeskAiService
{
    Task<string> GetCapabilitiesAsync(CancellationToken cancellationToken = default);
}
