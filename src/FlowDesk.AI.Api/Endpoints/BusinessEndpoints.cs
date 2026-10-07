using System.Security.Cryptography;
using System.Text;
using FlowDesk.AI.Application.Abstractions.Knowledge;

namespace FlowDesk.AI.Api.Endpoints;

public static class BusinessEndpoints
{
    private const string AdminHeader = "X-FlowDesk-Admin-Key";

    private sealed record CreateBusinessRequest(
        string Name,
        string? Slug);

    public static IEndpointRouteBuilder MapBusinessEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/businesses");

        group.MapPost("", async (
            CreateBusinessRequest request,
            HttpRequest httpRequest,
            IConfiguration configuration,
            IBusinessService businessService,
            CancellationToken cancellationToken) =>
        {
            if (!IsAdminRequest(httpRequest, configuration))
            {
                return Results.Unauthorized();
            }

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
            HttpRequest httpRequest,
            IConfiguration configuration,
            IBusinessService businessService,
            CancellationToken cancellationToken) =>
        {
            if (!IsAdminRequest(httpRequest, configuration))
            {
                return Results.Unauthorized();
            }

            var businesses = await businessService.GetAllAsync(cancellationToken);

            return Results.Ok(businesses);
        });

        return endpoints;
    }

    private static bool IsAdminRequest(
        HttpRequest request,
        IConfiguration configuration)
    {
        var configuredKey = configuration["FLOWDESK_ADMIN_KEY"];
        var providedKey = request.Headers[AdminHeader].ToString();

        if (string.IsNullOrWhiteSpace(configuredKey) ||
            string.IsNullOrWhiteSpace(providedKey))
        {
            return false;
        }

        var expected = Encoding.UTF8.GetBytes(configuredKey);
        var actual = Encoding.UTF8.GetBytes(providedKey);

        return expected.Length == actual.Length &&
               CryptographicOperations.FixedTimeEquals(expected, actual);
    }
}
