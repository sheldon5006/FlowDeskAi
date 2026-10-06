namespace FlowDesk.AI.Application.Knowledge;

public sealed record BusinessDto(
    Guid Id,
    string Name,
    string Slug,
    DateTimeOffset CreatedAtUtc);
