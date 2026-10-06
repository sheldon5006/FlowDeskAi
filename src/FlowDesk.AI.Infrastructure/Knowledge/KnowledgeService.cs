using FlowDesk.AI.Application.Abstractions.Knowledge;
using FlowDesk.AI.Application.Knowledge;
using FlowDesk.AI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FlowDesk.AI.Infrastructure.Knowledge;

public sealed class KnowledgeService(FlowDeskDbContext dbContext) : IKnowledgeService
{
    private const int ChunkSize = 1000;
    private const int ChunkOverlap = 150;

    public async Task<KnowledgeDocumentDto> AddDocumentAsync(
        string source,
        string content,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            throw new ArgumentException("Source is required.", nameof(source));
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Content is required.", nameof(content));
        }

        var document = new KnowledgeDocumentRecord
        {
            Id = Guid.NewGuid(),
            Source = source.Trim(),
            Content = content.Trim(),
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        foreach (var (chunkContent, chunkIndex) in CreateChunks(document.Content))
        {
            document.Chunks.Add(new KnowledgeChunkRecord
            {
                Id = Guid.NewGuid(),
                ChunkIndex = chunkIndex,
                Content = chunkContent,
                CreatedAtUtc = document.CreatedAtUtc
            });
        }

        dbContext.KnowledgeDocuments.Add(document);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new KnowledgeDocumentDto(
            document.Id,
            document.Source,
            document.Content,
            document.Chunks.Count,
            document.CreatedAtUtc);
    }

    public async Task<IReadOnlyList<KnowledgeDocumentDto>> GetDocumentsAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.KnowledgeDocuments
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new KnowledgeDocumentDto(
                x.Id,
                x.Source,
                x.Content,
                x.Chunks.Count,
                x.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }

    private static IEnumerable<(string Content, int Index)> CreateChunks(string content)
    {
        var normalized = content.Replace("\r\n", "\n").Trim();

        if (normalized.Length <= ChunkSize)
        {
            yield return (normalized, 0);
            yield break;
        }

        var index = 0;
        var start = 0;

        while (start < normalized.Length)
        {
            var length = Math.Min(ChunkSize, normalized.Length - start);
            var end = start + length;

            if (end < normalized.Length)
            {
                var boundary = normalized.LastIndexOfAny([' ', '\n', '.', ',', ';', ':', '!', '?'], end - 1, length);

                if (boundary > start + ChunkSize / 2)
                {
                    end = boundary + 1;
                }
            }

            var chunk = normalized[start..end].Trim();

            if (!string.IsNullOrWhiteSpace(chunk))
            {
                yield return (chunk, index++);
            }

            if (end >= normalized.Length)
            {
                break;
            }

            start = Math.Max(start + 1, end - ChunkOverlap);
        }
    }
}
