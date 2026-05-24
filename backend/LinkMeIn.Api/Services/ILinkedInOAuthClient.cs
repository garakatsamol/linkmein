using LinkMeIn.Api.Options;

namespace LinkMeIn.Api.Services;

public interface ILinkedInOAuthClient
{
    Task<LinkedInTokenResponse> ExchangeCodeForTokenAsync(
        string code,
        LinkedInOptions options,
        CancellationToken cancellationToken = default);
}
