namespace FlowDesk.AI.Application.Abstractions.Knowledge;

public interface IBusinessApiKeyService
{
    Task<BusinessApiKeyResult> CreateAsync(
        Guid businessId,
        CancellationToken cancellationToken = default);

    Task<Guid?> ResolveBusinessIdAsync(
        string apiKey,
        CancellationToken cancellationToken = default);
}

public sealed record BusinessApiKeyResult(
    Guid BusinessId,
    string ApiKey);
