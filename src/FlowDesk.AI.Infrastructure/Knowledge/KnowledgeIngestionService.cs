using FlowDesk.AI.Application.Abstractions.Knowledge;
using FlowDesk.AI.Application.Knowledge;

namespace FlowDesk.AI.Infrastructure.Knowledge;

public sealed class KnowledgeIngestionService(
    IKnowledgeService knowledgeService,
    IKnowledgeEmbeddingService embeddingService) : IKnowledgeIngestionService
{
    public async Task<KnowledgeIngestionResultDto> IngestAsync(
        string source,
        string content,
        CancellationToken cancellationToken = default)
    {
        var document = await knowledgeService.AddDocumentAsync(
            source,
            content,
            cancellationToken);

        var embeddedChunkCount = await embeddingService.EmbedDocumentAsync(
            document.Id,
            cancellationToken);

        return new KnowledgeIngestionResultDto(
            document,
            embeddedChunkCount);
    }
}
