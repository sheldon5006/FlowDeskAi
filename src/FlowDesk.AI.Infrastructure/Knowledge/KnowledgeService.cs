using FlowDesk.AI.Application.Abstractions.Knowledge;
using FlowDesk.AI.Application.Knowledge;
using FlowDesk.AI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FlowDesk.AI.Infrastructure.Knowledge;

public sealed class KnowledgeService(FlowDeskDbContext dbContext) : IKnowledgeService
{
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

        dbContext.KnowledgeDocuments.Add(document);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new KnowledgeDocumentDto(
            document.Id,
            document.Source,
            document.Content,
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
                x.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }
}
