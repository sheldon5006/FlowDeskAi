using System.Text.RegularExpressions;
using FlowDesk.AI.Application.Abstractions.Knowledge;
using FlowDesk.AI.Application.Knowledge;
using FlowDesk.AI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FlowDesk.AI.Infrastructure.Knowledge;

public sealed class BusinessService(FlowDeskDbContext dbContext) : IBusinessService
{
    public async Task<BusinessDto> CreateAsync(
        string name,
        string? slug = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Business name is required.", nameof(name));
        }

        var normalizedName = name.Trim();
        var normalizedSlug = string.IsNullOrWhiteSpace(slug)
            ? CreateSlug(normalizedName)
            : CreateSlug(slug);

        if (string.IsNullOrWhiteSpace(normalizedSlug))
        {
            throw new ArgumentException("Business slug is required.", nameof(slug));
        }

        var exists = await dbContext.Businesses
            .AnyAsync(x => x.Slug == normalizedSlug, cancellationToken);

        if (exists)
        {
            throw new ArgumentException(
                $"A business with slug '{normalizedSlug}' already exists.",
                nameof(slug));
        }

        var business = new BusinessRecord
        {
            Id = Guid.NewGuid(),
            Name = normalizedName,
            Slug = normalizedSlug,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        dbContext.Businesses.Add(business);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new BusinessDto(
            business.Id,
            business.Name,
            business.Slug,
            business.CreatedAtUtc);
    }

    public async Task<IReadOnlyList<BusinessDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Businesses
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new BusinessDto(
                x.Id,
                x.Name,
                x.Slug,
                x.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }

    private static string CreateSlug(string value)
    {
        var slug = Regex.Replace(value.Trim().ToLowerInvariant(), @"[^a-z0-9]+", "-");
        return slug.Trim('-');
    }
}
