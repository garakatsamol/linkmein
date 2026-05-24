using System.Net.Http.Json;
using LinkMeIn.Api.Options;

namespace LinkMeIn.Api.Services;

public class LinkedInOAuthClient(HttpClient httpClient) : ILinkedInOAuthClient
{
    public async Task<LinkedInTokenResponse> ExchangeCodeForTokenAsync(
        string code,
        LinkedInOptions options,
        CancellationToken cancellationToken = default)
    {
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["redirect_uri"] = options.RedirectUri,
            ["client_id"] = options.ClientId,
            ["client_secret"] = options.ClientSecret
        });

        using var response = await httpClient.PostAsync(options.TokenEndpoint, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync<LinkedInTokenResponse>(cancellationToken);
        if (tokenResponse == null || string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
        {
            throw new InvalidOperationException("LinkedIn token response did not include an access token.");
        }

        return tokenResponse;
    }
}
