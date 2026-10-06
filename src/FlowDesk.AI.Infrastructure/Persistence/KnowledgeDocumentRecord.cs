namespace FlowDesk.AI.Infrastructure.Persistence;

public sealed class KnowledgeDocumentRecord
{
    public Guid Id { get; set; }

    public string Source { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; set; }

    public ICollection<KnowledgeChunkRecord> Chunks { get; set; } = [];
}
