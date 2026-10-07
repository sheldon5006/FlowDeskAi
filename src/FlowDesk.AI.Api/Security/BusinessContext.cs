using System.Security.Claims;

namespace FlowDesk.AI.Api.Security;

public static class BusinessContext
{
    public static Guid GetRequiredBusinessId(ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(
            BusinessApiKeyAuthenticationHandler.BusinessIdClaim);

        return Guid.TryParse(value, out var businessId) && businessId != Guid.Empty
            ? businessId
            : throw new UnauthorizedAccessException("The API key does not identify a business.");
    }
}
