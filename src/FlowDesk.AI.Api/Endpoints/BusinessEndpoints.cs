using FlowDesk.AI.Application.Abstractions.Knowledge;

namespace FlowDesk.AI.Api.Endpoints;

public static class BusinessEndpoints
{
    private sealed record CreateBusinessRequest(
        string Name,
        string? Slug);

    public static IEndpointRouteBuilder MapBusinessEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/businesses");

        group.MapPost("", async (
            CreateBusinessRequest request,
            IBusinessService businessService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var business = await businessService.CreateAsync(
                    request.Name,
                    request.Slug,
                    cancellationToken);

                return Results.Created(
                    $"/api/businesses/{business.Id}",
                    business);
            }
            catch (ArgumentException exception)
            {
                return Results.BadRequest(new { error = exception.Message });
            }
        });

        group.MapGet("", async (
            IBusinessService businessService,
            CancellationToken cancellationToken) =>
        {
            var businesses = await businessService.GetAllAsync(cancellationToken);

            return Results.Ok(businesses);
        });

        return endpoints;
    }
}
