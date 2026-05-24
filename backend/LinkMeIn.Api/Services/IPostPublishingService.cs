namespace LinkMeIn.Api.Services;

public interface IPostPublishingService
{
    Task<PublishPostResult> PublishTextPostAsync(
        Guid postId,
        string ownerId,
        CancellationToken cancellationToken = default);
}
