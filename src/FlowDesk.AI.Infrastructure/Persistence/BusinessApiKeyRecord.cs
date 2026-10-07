namespace FlowDesk.AI.Infrastructure.Persistence;

public sealed class BusinessApiKeyRecord
{
    public Guid Id { get; set; }

    public Guid BusinessId { get; set; }

    public string KeyHash { get; set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset? RevokedAtUtc { get; set; }

    public DateTimeOffset? LastUsedAtUtc { get; set; }

    public BusinessRecord Business { get; set; } = null!;
}
