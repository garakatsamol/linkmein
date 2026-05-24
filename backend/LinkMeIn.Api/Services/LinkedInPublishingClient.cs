using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
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
        AddLinkedInHeaders(request, accessToken, options);
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

        return await SendPostRequestAsync(request, cancellationToken);
    }

    public async Task<LinkedInImageUploadResponse> UploadImageAsync(
        string accessToken,
        string ownerUrn,
        Stream imageContent,
        string contentType,
        string fileName,
        LinkedInOptions options,
        CancellationToken cancellationToken = default)
    {
        using var initializeRequest = new HttpRequestMessage(HttpMethod.Post, options.ImagesInitializeUploadEndpoint);
        AddLinkedInHeaders(initializeRequest, accessToken, options);
        initializeRequest.Content = JsonContent.Create(new
        {
            initializeUploadRequest = new
            {
                owner = ownerUrn
            }
        });

        using var initializeResponse = await httpClient.SendAsync(initializeRequest, cancellationToken);
        var initializeSummary = ResponseSummary(initializeResponse);
        var initializeBody = await initializeResponse.Content.ReadAsStringAsync(cancellationToken);
        if (!initializeResponse.IsSuccessStatusCode)
        {
            throw new LinkedInPublishingException(SanitizeErrorSummary(initializeSummary, initializeBody));
        }

        var (uploadUrl, imageUrn) = ParseInitializeUploadResponse(initializeBody);
        using var uploadRequest = new HttpRequestMessage(HttpMethod.Put, uploadUrl);
        AddLinkedInHeaders(uploadRequest, accessToken, options);
        uploadRequest.Content = new StreamContent(imageContent);
        uploadRequest.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        using var uploadResponse = await httpClient.SendAsync(uploadRequest, cancellationToken);
        var uploadSummary = ResponseSummary(uploadResponse);
        if (!uploadResponse.IsSuccessStatusCode)
        {
            var uploadBody = await uploadResponse.Content.ReadAsStringAsync(cancellationToken);
            throw new LinkedInPublishingException(SanitizeErrorSummary(uploadSummary, uploadBody));
        }

        return new LinkedInImageUploadResponse
        {
            ImageUrn = imageUrn,
            ResponseSummary = uploadSummary
        };
    }

    public async Task<LinkedInPublishResponse> PublishSingleImagePostAsync(
        string accessToken,
        string authorUrn,
        string commentary,
        string imageUrn,
        LinkedInOptions options,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, options.PostsEndpoint);
        AddLinkedInHeaders(request, accessToken, options);
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
            content = new
            {
                media = new
                {
                    id = imageUrn
                }
            },
            lifecycleState = "PUBLISHED",
            isReshareDisabledByAuthor = false
        });

        return await SendPostRequestAsync(request, cancellationToken);
    }

    private static void AddLinkedInHeaders(HttpRequestMessage request, string accessToken, LinkedInOptions options)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Add("X-Restli-Protocol-Version", "2.0.0");
        request.Headers.Add("Linkedin-Version", options.ApiVersion);
    }

    private static (string UploadUrl, string ImageUrn) ParseInitializeUploadResponse(string responseBody)
    {
        using var document = JsonDocument.Parse(responseBody);
        if (!document.RootElement.TryGetProperty("value", out var value)
            || !value.TryGetProperty("uploadUrl", out var uploadUrlElement)
            || !value.TryGetProperty("image", out var imageElement))
        {
            throw new LinkedInPublishingException("LinkedIn image upload initialization response was missing upload details.");
        }

        var uploadUrl = uploadUrlElement.GetString();
        var imageUrn = imageElement.GetString();
        if (string.IsNullOrWhiteSpace(uploadUrl) || string.IsNullOrWhiteSpace(imageUrn))
        {
            throw new LinkedInPublishingException("LinkedIn image upload initialization response returned empty upload details.");
        }

        return (uploadUrl, imageUrn);
    }

    private async Task<LinkedInPublishResponse> SendPostRequestAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        using var response = await httpClient.SendAsync(request, cancellationToken);
        var responseSummary = ResponseSummary(response);

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

    private static string ResponseSummary(HttpResponseMessage response)
    {
        return $"{(int)response.StatusCode} {response.ReasonPhrase}";
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
