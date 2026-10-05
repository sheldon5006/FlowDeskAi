using Pgvector;

namespace FlowDesk.AI.Infrastructure.Persistence;

public sealed class KnowledgeDocumentRecord
{
    public Guid Id { get; set; }

    public string Source { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public Vector? Embedding { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }
}
