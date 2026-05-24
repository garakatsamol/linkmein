using System.Net.Http.Headers;
using System.Net.Http.Json;
using LinkMeIn.Api.Options;

namespace LinkMeIn.Api.Services;

public class LinkedInPublishingClient(HttpClient httpClient) : ILinkedInPublishingClient
{
    public async Task<LinkedInPublishResponse> PublishTextPostAsync(
        string accessToken,
        string authorUrn,
        string commentary,
        LinkedInOptions options,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, options.PostsEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Add("X-Restli-Protocol-Version", "2.0.0");
        request.Headers.Add("Linkedin-Version", options.ApiVersion);
        request.Content = JsonContent.Create(new
        {
            author = authorUrn,
            commentary,
            visibility = "PUBLIC",
            distribution = new
            {
                feedDistribution = "MAIN_FEED",
                targetEntities = Array.Empty<object>(),
                thirdPartyDistributionChannels = Array.Empty<object>()
            },
            lifecycleState = "PUBLISHED",
            isReshareDisabledByAuthor = false
        });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        var responseSummary = $"{(int)response.StatusCode} {response.ReasonPhrase}";

        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new LinkedInPublishingException(SanitizeErrorSummary(responseSummary, responseBody));
        }

        if (!response.Headers.TryGetValues("x-restli-id", out var values))
        {
            throw new LinkedInPublishingException("LinkedIn publish succeeded but did not return x-restli-id.");
        }

        var linkedInPostId = values.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(linkedInPostId))
        {
            throw new LinkedInPublishingException("LinkedIn publish succeeded but returned an empty x-restli-id.");
        }

        return new LinkedInPublishResponse
        {
            LinkedInPostId = linkedInPostId,
            ResponseSummary = responseSummary
        };
    }

    private static string SanitizedSnippet(string value)
    {
        return value.Length <= 500 ? value : value[..500];
    }

    private static string SanitizeErrorSummary(string responseSummary, string responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return responseSummary;
        }

        return $"{responseSummary}: {SanitizedSnippet(responseBody)}";
    }
}
