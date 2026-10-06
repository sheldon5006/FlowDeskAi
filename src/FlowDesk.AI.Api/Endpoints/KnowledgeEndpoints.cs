using FlowDesk.AI.Application.Abstractions.Knowledge;

namespace FlowDesk.AI.Api.Endpoints;

public static class KnowledgeEndpoints
{
    private sealed record CreateKnowledgeDocumentRequest(
        string Source,
        string Content);

    public static IEndpointRouteBuilder MapKnowledgeEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/knowledge");

        group.MapPost("/documents", async (
            CreateKnowledgeDocumentRequest request,
            IKnowledgeService knowledgeService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var document = await knowledgeService.AddDocumentAsync(
                    request.Source,
                    request.Content,
                    cancellationToken);

                return Results.Created(
                    $"/api/knowledge/documents/{document.Id}",
                    document);
            }
            catch (ArgumentException exception)
            {
                return Results.BadRequest(new { error = exception.Message });
            }
        });

        group.MapGet("/documents", async (
            IKnowledgeService knowledgeService,
            CancellationToken cancellationToken) =>
        {
            var documents = await knowledgeService.GetDocumentsAsync(cancellationToken);

            return Results.Ok(documents);
        });

        group.MapGet("/documents/{documentId:guid}/chunks", async (
            Guid documentId,
            IKnowledgeService knowledgeService,
            CancellationToken cancellationToken) =>
        {
            var chunks = await knowledgeService.GetChunksAsync(
                documentId,
                cancellationToken);

            return Results.Ok(chunks);
        });

        return endpoints;
    }
}
