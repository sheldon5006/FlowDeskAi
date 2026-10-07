using System.Security.Claims;
using System.Text.Encodings.Web;
using FlowDesk.AI.Application.Abstractions.Knowledge;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace FlowDesk.AI.Api.Security;

public sealed class BusinessApiKeyAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IBusinessApiKeyService apiKeyService)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "BusinessApiKey";
    public const string ApiKeyHeader = "X-FlowDesk-Api-Key";
    public const string BusinessIdClaim = "business_id";

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(ApiKeyHeader, out var apiKey))
        {
            return AuthenticateResult.NoResult();
        }

        var businessId = await apiKeyService.ResolveBusinessIdAsync(
            apiKey.ToString(),
            Context.RequestAborted);

        if (businessId is null)
        {
            return AuthenticateResult.Fail("Invalid or revoked FlowDesk API key.");
        }

        var claims = new[]
        {
            new Claim(BusinessIdClaim, businessId.Value.ToString())
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return AuthenticateResult.Success(ticket);
    }
}
