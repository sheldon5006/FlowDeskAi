using Pgvector;

namespace FlowDesk.AI.Infrastructure.Persistence;

public sealed class KnowledgeChunkRecord
{
    public Guid Id { get; set; }

    public Guid KnowledgeDocumentId { get; set; }

    public int ChunkIndex { get; set; }

    public string Content { get; set; } = string.Empty;

    public Vector? Embedding { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public KnowledgeDocumentRecord KnowledgeDocument { get; set; } = null!;
}
