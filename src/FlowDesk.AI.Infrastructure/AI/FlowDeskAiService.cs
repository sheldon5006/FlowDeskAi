using FlowDesk.AI.Application.Abstractions.AI;

namespace FlowDesk.AI.Infrastructure.AI;

public sealed class FlowDeskAiService : IFlowDeskAiService
{
    public Task<string> GetCapabilitiesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult("FlowDesk AI foundation is ready. RAG capabilities will be added next.");
    }
}
