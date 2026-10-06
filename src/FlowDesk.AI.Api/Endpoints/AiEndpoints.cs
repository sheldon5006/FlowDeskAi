using FlowDesk.AI.Application.Abstractions.AI;

namespace FlowDesk.AI.Api.Endpoints;

public static class AiEndpoints
{
    private sealed record ChatRequest(string Message, int? TopK);

    public static IEndpointRouteBuilder MapAiEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/ai/chat", async (
            ChatRequest request,
            IFlowDeskAiService aiService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Message))
                {
                    return Results.BadRequest(new { error = "Message is required." });
                }

                var result = await aiService.AskAsync(
                    request.Message,
                    request.TopK ?? 5,
                    cancellationToken);

                return Results.Ok(result);
            }
            catch (ArgumentException exception)
            {
                return Results.BadRequest(new { error = exception.Message });
            }
            catch (InvalidOperationException exception)
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status503ServiceUnavailable,
                    title: "AI generation failed",
                    detail: exception.Message);
            }
            catch (HttpRequestException exception)
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status503ServiceUnavailable,
                    title: "AI provider request failed",
                    detail: exception.Message);
            }
        });

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
