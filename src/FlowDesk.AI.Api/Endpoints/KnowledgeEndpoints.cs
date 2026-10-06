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

        group.MapPost("/ingest", async (
            CreateKnowledgeDocumentRequest request,
            IKnowledgeIngestionService ingestionService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await ingestionService.IngestAsync(
                    request.Source,
                    request.Content,
                    cancellationToken);

                return Results.Created(
                    $"/api/knowledge/documents/{result.Document.Id}",
                    result);
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
            IKnowledgeIngestionService ingestionService,
            CancellationToken cancellationToken) =>
        {
            if (file is null || file.Length == 0)
            {
                return Results.BadRequest(new { error = "A non-empty file is required." });
            }

            if (file.Length > 5 * 1024 * 1024)
            {
                return Results.BadRequest(new { error = "The file must be 5 MB or smaller." });
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var supportedExtensions = new[] { ".txt", ".md", ".csv", ".json" };

            if (!supportedExtensions.Contains(extension))
            {
                return Results.BadRequest(new
                {
                    error = "Supported file types are .txt, .md, .csv, and .json."
                });
            }

            try
            {
                using var reader = new StreamReader(file.OpenReadStream());
                var content = await reader.ReadToEndAsync(cancellationToken);

                var result = await ingestionService.IngestAsync(
                    file.FileName,
                    content,
                    cancellationToken);

                return Results.Created(
                    $"/api/knowledge/documents/{result.Document.Id}",
                    result);
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

        group.MapGet("/search", async (
            string query,
            int? topK,
            IKnowledgeSearchService searchService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var results = await searchService.SearchAsync(
                    query,
                    topK ?? 5,
                    cancellationToken);

                return Results.Ok(results);
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
            IKnowledgeEmbeddingService embeddingService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var embeddedChunkCount = await embeddingService.EmbedDocumentAsync(
                    documentId,
                    cancellationToken);

                return Results.Ok(new
                {
                    documentId,
                    embeddedChunkCount
                });
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
