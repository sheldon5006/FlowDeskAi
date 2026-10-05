using FlowDesk.AI.Application.Abstractions.AI;

namespace FlowDesk.AI.Api.Endpoints;

public static class AiEndpoints
{
    public static IEndpointRouteBuilder MapAiEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/ai/capabilities", async (
            IFlowDeskAiService aiService,
            CancellationToken cancellationToken) =>
        {
            var message = await aiService.GetCapabilitiesAsync(cancellationToken);

            return Results.Ok(new
            {
                service = "FlowDesk AI",
                message
            });
        });

        return endpoints;
    }
}
