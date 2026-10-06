namespace FlowDesk.AI.Infrastructure.Persistence;

public sealed class KnowledgeDocumentRecord
{
    public Guid Id { get; set; }

    public Guid BusinessId { get; set; }

    public string Source { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; set; }

    public BusinessRecord Business { get; set; } = null!;

    public ICollection<KnowledgeChunkRecord> Chunks { get; set; } = [];
}
