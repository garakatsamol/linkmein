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

    Task<LinkedInImageUploadResponse> UploadImageAsync(
        string accessToken,
        string ownerUrn,
        Stream imageContent,
        string contentType,
        string fileName,
        LinkedInOptions options,
        CancellationToken cancellationToken = default);

    Task<LinkedInPublishResponse> PublishSingleImagePostAsync(
        string accessToken,
        string authorUrn,
        string commentary,
        string imageUrn,
        LinkedInOptions options,
        CancellationToken cancellationToken = default);
}
