using LinkMeIn.Api.Options;

namespace LinkMeIn.Api.Services;

public interface ILinkedInPublishingClient
{
    Task<LinkedInPublishResponse> PublishTextPostAsync(
        string accessToken,
        string authorUrn,
        string commentary,
        LinkedInOptions options,
        CancellationToken cancellationToken = default);
}
