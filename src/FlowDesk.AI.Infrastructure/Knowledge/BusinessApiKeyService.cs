using System.Security.Cryptography;
using System.Text;
using FlowDesk.AI.Application.Abstractions.Knowledge;
using FlowDesk.AI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FlowDesk.AI.Infrastructure.Knowledge;

public sealed class BusinessApiKeyService(
    FlowDeskDbContext dbContext) : IBusinessApiKeyService
{
    public async Task<BusinessApiKeyResult> CreateAsync(
        Guid businessId,
        CancellationToken cancellationToken = default)
    {
        if (businessId == Guid.Empty)
        {
            throw new ArgumentException("Business ID is required.", nameof(businessId));
        }

        var businessExists = await dbContext.Businesses
            .AnyAsync(x => x.Id == businessId, cancellationToken);

        if (!businessExists)
        {
            throw new ArgumentException(
                $"Business '{businessId}' was not found.",
                nameof(businessId));
        }

        var apiKey = $"fdai_{Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=')}";

        var record = new BusinessApiKeyRecord
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            KeyHash = Hash(apiKey),
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        dbContext.BusinessApiKeys.Add(record);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new BusinessApiKeyResult(businessId, apiKey);
    }

    public async Task<Guid?> ResolveBusinessIdAsync(
        string apiKey,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return null;
        }

        var hash = Hash(apiKey.Trim());

        var record = await dbContext.BusinessApiKeys
            .SingleOrDefaultAsync(
                x => x.KeyHash == hash && x.RevokedAtUtc == null,
                cancellationToken);

        if (record is null)
        {
            return null;
        }

        record.LastUsedAtUtc = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return record.BusinessId;
    }

    private static string Hash(string value)
    {
        return Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    }
}
