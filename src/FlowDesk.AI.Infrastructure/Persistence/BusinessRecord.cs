namespace FlowDesk.AI.Infrastructure.Persistence;

public sealed class BusinessRecord
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; set; }

    public ICollection<KnowledgeDocumentRecord> KnowledgeDocuments { get; set; } = [];
}
