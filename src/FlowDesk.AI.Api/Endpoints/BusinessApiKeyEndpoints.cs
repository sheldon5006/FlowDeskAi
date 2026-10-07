using System.Security.Cryptography;
using System.Text;
using FlowDesk.AI.Application.Abstractions.Knowledge;

namespace FlowDesk.AI.Api.Endpoints;

public static class BusinessApiKeyEndpoints
{
    private const string AdminHeader = "X-FlowDesk-Admin-Key";

    public static IEndpointRouteBuilder MapBusinessApiKeyEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/admin/businesses/{businessId:guid}/api-keys", async (
            Guid businessId,
            HttpRequest request,
            IConfiguration configuration,
            IBusinessApiKeyService apiKeyService,
            CancellationToken cancellationToken) =>
        {
            if (!IsAdminRequest(request, configuration))
            {
                return Results.Unauthorized();
            }

            try
            {
                var result = await apiKeyService.CreateAsync(
                    businessId,
                    cancellationToken);

                return Results.Ok(new
                {
                    businessId = result.BusinessId,
                    apiKey = result.ApiKey,
                    warning = "Store this API key securely. It cannot be retrieved later."
                });
            }
            catch (ArgumentException exception)
            {
                return Results.BadRequest(new { error = exception.Message });
            }
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
