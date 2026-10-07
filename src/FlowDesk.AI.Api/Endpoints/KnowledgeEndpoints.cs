using FlowDesk.AI.Api.Security;
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
        var group = endpoints
            .MapGroup("/api/knowledge")
            .RequireAuthorization();

        group.MapPost("/ingest", async (
            CreateKnowledgeDocumentRequest request,
            HttpContext httpContext,
            IKnowledgeIngestionService ingestionService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var businessId = BusinessContext.GetRequiredBusinessId(httpContext.User);

                var result = await ingestionService.IngestAsync(
                    businessId,
                    request.Source,
                    request.Content,
                    cancellationToken);

                return Results.Created(
                    $"/api/knowledge/documents/{result.Document.Id}",
                    result);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
            catch (ArgumentException exception)
            {
                return Results.BadRequest(new { error = exception.Message });
            }
            catch (InvalidOperationException exception)
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status503ServiceUnavailable,
                    title: "Knowledge ingestion failed",
                    detail: exception.Message);
            }
            catch (HttpRequestException exception)
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status503ServiceUnavailable,
                    title: "Embedding provider request failed",
                    detail: exception.Message);
            }
        });

        group.MapPost("/ingest-file", async (
            IFormFile file,
            HttpContext httpContext,
            IKnowledgeIngestionService ingestionService,
            CancellationToken cancellationToken) =>
        {
            if (file is null || file.Length == 0)
            {
                return Results.BadRequest(new { error = "A non-empty file is required." });
            }

            if (file.Length > 10 * 1024 * 1024)
            {
                return Results.BadRequest(new { error = "The file must be 10 MB or smaller." });
            }

            try
            {
                var businessId = BusinessContext.GetRequiredBusinessId(httpContext.User);

                await using var stream = file.OpenReadStream();

                var result = await ingestionService.IngestFileAsync(
                    businessId,
                    file.FileName,
                    stream,
                    cancellationToken);

                return Results.Created(
                    $"/api/knowledge/documents/{result.Document.Id}",
                    result);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
            catch (ArgumentException exception)
            {
                return Results.BadRequest(new { error = exception.Message });
            }
            catch (InvalidOperationException exception)
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status503ServiceUnavailable,
                    title: "Knowledge ingestion failed",
                    detail: exception.Message);
            }
            catch (HttpRequestException exception)
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status503ServiceUnavailable,
                    title: "Embedding provider request failed",
                    detail: exception.Message);
            }
        }).DisableAntiforgery();

        group.MapPost("/documents", async (
            CreateKnowledgeDocumentRequest request,
            HttpContext httpContext,
            IKnowledgeService knowledgeService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var businessId = BusinessContext.GetRequiredBusinessId(httpContext.User);

                var document = await knowledgeService.AddDocumentAsync(
                    businessId,
                    request.Source,
                    request.Content,
                    cancellationToken);

                return Results.Created(
                    $"/api/knowledge/documents/{document.Id}",
                    document);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
            catch (ArgumentException exception)
            {
                return Results.BadRequest(new { error = exception.Message });
            }
        });

        group.MapGet("/documents", async (
            HttpContext httpContext,
            IKnowledgeService knowledgeService,
            CancellationToken cancellationToken) =>
        {
            var businessId = BusinessContext.GetRequiredBusinessId(httpContext.User);

            var documents = await knowledgeService.GetDocumentsAsync(
                businessId,
                cancellationToken);

            return Results.Ok(documents);
        });

        group.MapGet("/documents/{documentId:guid}/chunks", async (
            Guid documentId,
            HttpContext httpContext,
            IKnowledgeService knowledgeService,
            CancellationToken cancellationToken) =>
        {
            var businessId = BusinessContext.GetRequiredBusinessId(httpContext.User);

            var chunks = await knowledgeService.GetChunksAsync(
                businessId,
                documentId,
                cancellationToken);

            return Results.Ok(chunks);
        });

        group.MapGet("/search", async (
            string query,
            int? topK,
            HttpContext httpContext,
            IKnowledgeSearchService searchService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var businessId = BusinessContext.GetRequiredBusinessId(httpContext.User);

                var results = await searchService.SearchAsync(
                    businessId,
                    query,
                    topK ?? 5,
                    cancellationToken);

                return Results.Ok(results);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
            catch (ArgumentException exception)
            {
                return Results.BadRequest(new { error = exception.Message });
            }
            catch (InvalidOperationException exception)
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status503ServiceUnavailable,
                    title: "Knowledge search failed",
                    detail: exception.Message);
            }
            catch (HttpRequestException exception)
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status503ServiceUnavailable,
                    title: "Embedding provider request failed",
                    detail: exception.Message);
            }
        });

        group.MapPost("/documents/{documentId:guid}/embed", async (
            Guid documentId,
            HttpContext httpContext,
            IKnowledgeEmbeddingService embeddingService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var businessId = BusinessContext.GetRequiredBusinessId(httpContext.User);

                var embeddedChunkCount = await embeddingService.EmbedDocumentAsync(
                    businessId,
                    documentId,
                    cancellationToken);

                return Results.Ok(new
                {
                    businessId,
                    documentId,
                    embeddedChunkCount
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
            catch (ArgumentException exception)
            {
                return Results.BadRequest(new { error = exception.Message });
            }
            catch (InvalidOperationException exception)
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status503ServiceUnavailable,
                    title: "Embedding generation failed",
                    detail: exception.Message);
            }
            catch (HttpRequestException exception)
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status503ServiceUnavailable,
                    title: "Embedding provider request failed",
                    detail: exception.Message);
            }
        });

        return endpoints;
    }
}
