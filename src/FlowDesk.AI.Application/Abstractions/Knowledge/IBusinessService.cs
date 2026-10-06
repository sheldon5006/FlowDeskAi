using FlowDesk.AI.Application.Knowledge;

namespace FlowDesk.AI.Application.Abstractions.Knowledge;

public interface IBusinessService
{
    Task<BusinessDto> CreateAsync(
        string name,
        string? slug = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BusinessDto>> GetAllAsync(
        CancellationToken cancellationToken = default);
}
