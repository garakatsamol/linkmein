using LinkMeIn.Api.Data;
using LinkMeIn.Api.Entities;
using LinkMeIn.Api.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LinkMeIn.Api.Services;

public class PostPublishingService : IPostPublishingService
{
    private readonly LinkMeInDbContext _db;
    private readonly ILinkedInPublishingClient _linkedInPublishingClient;
    private readonly IMediaStorageService _mediaStorage;
    private readonly ITokenEncryptionService _tokenEncryption;
    private readonly LinkedInOptions _linkedInOptions;

    public PostPublishingService(
        LinkMeInDbContext db,
        ILinkedInPublishingClient linkedInPublishingClient,
        IMediaStorageService mediaStorage,
        ITokenEncryptionService tokenEncryption,
        IOptions<LinkedInOptions> linkedInOptions)
    {
        _db = db;
        _linkedInPublishingClient = linkedInPublishingClient;
        _mediaStorage = mediaStorage;
        _tokenEncryption = tokenEncryption;
        _linkedInOptions = linkedInOptions.Value;
    }

    public async Task<PublishPostResult> PublishTextPostAsync(
        Guid postId,
        string ownerId,
        CancellationToken cancellationToken = default)
    {
        var post = await _db.Posts.FirstOrDefaultAsync(
            item => item.Id == postId && item.OwnerId == ownerId,
            cancellationToken);
        if (post == null)
        {
            return Failure(404, "Post not found.");
        }

        var connection = await _db.LinkedInConnections
            .Where(item => item.OwnerId == ownerId && item.DisconnectedAt == null)
            .OrderByDescending(item => item.ConnectedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (connection == null)
        {
            await RecordFailureAsync(post, "No active LinkedIn connection found.", cancellationToken);
            return Failure(400, "No active LinkedIn connection found.");
        }

        if (string.IsNullOrWhiteSpace(connection.LinkedInMemberId))
        {
            const string message = "LinkedIn member URN is missing. Reconnect LinkedIn before publishing.";
            await RecordFailureAsync(post, message, cancellationToken);
            return Failure(400, message);
        }

        var mediaItems = await _db.PostMedia
            .Where(item => item.PostId == post.Id && item.OwnerId == ownerId)
            .OrderBy(item => item.CreatedAt)
            .ToListAsync(cancellationToken);

        if (mediaItems.Count > 1)
        {
            const string message = "Publishing multiple images is not supported yet.";
            await RecordFailureAsync(post, message, "LinkedIn multi-image post publish", cancellationToken);
            return Failure(400, message);
        }

        var requestSummary = mediaItems.Count == 1
            ? "LinkedIn single-image post publish"
            : "LinkedIn text-only post publish";

        var now = DateTimeOffset.UtcNow;
        post.Status = PostStatus.Publishing;
        post.UpdatedAt = now;
        await _db.SaveChangesAsync(cancellationToken);

        var publishAttempt = new PublishAttempt
        {
            Id = Guid.NewGuid(),
            PostId = post.Id,
            OwnerId = ownerId,
            Status = PublishAttemptStatus.Pending,
            AttemptedAt = now,
            RequestSummary = requestSummary
        };
        _db.PublishAttempts.Add(publishAttempt);

        try
        {
            var accessToken = _tokenEncryption.Unprotect(connection.AccessTokenEncrypted);
            LinkedInPublishResponse response;
            LinkedInImageUploadResponse? imageUploadResponse = null;
            if (mediaItems.Count == 1)
            {
                var media = mediaItems[0];
                using var mediaStream = await _mediaStorage.OpenReadAsync(media.StoragePath, cancellationToken);
                if (mediaStream == null)
                {
                    const string message = "Local media file is missing. Re-upload the image before publishing.";
                    await RecordPublishExceptionAsync(post, publishAttempt, message, cancellationToken);
                    return Failure(400, message);
                }

                imageUploadResponse = await _linkedInPublishingClient.UploadImageAsync(
                    accessToken,
                    connection.LinkedInMemberId,
                    mediaStream,
                    media.ContentType,
                    media.FileName,
                    _linkedInOptions,
                    cancellationToken);

                media.LinkedInAssetUrn = imageUploadResponse.ImageUrn;
                await _db.SaveChangesAsync(cancellationToken);

                response = await _linkedInPublishingClient.PublishSingleImagePostAsync(
                    accessToken,
                    connection.LinkedInMemberId,
                    post.Content,
                    imageUploadResponse.ImageUrn,
                    _linkedInOptions,
                    cancellationToken);
            }
            else
            {
                response = await _linkedInPublishingClient.PublishTextPostAsync(
                    accessToken,
                    connection.LinkedInMemberId,
                    post.Content,
                    _linkedInOptions,
                    cancellationToken);
            }

            var publishedAt = DateTimeOffset.UtcNow;
            post.Status = PostStatus.Published;
            post.PublishedAt = publishedAt;
            post.LinkedInPostId = response.LinkedInPostId;
            post.UpdatedAt = publishedAt;

            publishAttempt.Status = PublishAttemptStatus.Succeeded;
            publishAttempt.LinkedInPostId = response.LinkedInPostId;
            publishAttempt.ResponseSummary = imageUploadResponse == null
                ? response.ResponseSummary
                : $"{imageUploadResponse.ResponseSummary}; {response.ResponseSummary}";

            await _db.SaveChangesAsync(cancellationToken);

            return new PublishPostResult
            {
                Success = true,
                StatusCode = 200,
                Message = "Post published to LinkedIn.",
                LinkedInPostId = response.LinkedInPostId
            };
        }
        catch (LinkedInPublishingException exception)
        {
            await RecordPublishExceptionAsync(post, publishAttempt, exception.SafeMessage, cancellationToken);
            return Failure(502, "LinkedIn publish failed.");
        }
        catch (Exception)
        {
            await RecordPublishExceptionAsync(post, publishAttempt, "LinkedIn publish failed.", cancellationToken);
            return Failure(502, "LinkedIn publish failed.");
        }
    }

    private async Task RecordFailureAsync(
        Post post,
        string message,
        string requestSummary,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        post.Status = PostStatus.Failed;
        post.UpdatedAt = now;
        _db.PublishAttempts.Add(new PublishAttempt
        {
            Id = Guid.NewGuid(),
            PostId = post.Id,
            OwnerId = post.OwnerId,
            Status = PublishAttemptStatus.Failed,
            AttemptedAt = now,
            ErrorMessage = message,
            RequestSummary = requestSummary
        });
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task RecordFailureAsync(Post post, string message, CancellationToken cancellationToken)
    {
        await RecordFailureAsync(post, message, "LinkedIn text-only post publish", cancellationToken);
    }

    private async Task RecordPublishExceptionAsync(
        Post post,
        PublishAttempt publishAttempt,
        string safeErrorMessage,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        post.Status = PostStatus.Failed;
        post.UpdatedAt = now;
        publishAttempt.Status = PublishAttemptStatus.Failed;
        publishAttempt.ErrorMessage = safeErrorMessage;
        await _db.SaveChangesAsync(cancellationToken);
    }

    private static PublishPostResult Failure(int statusCode, string message)
    {
        return new PublishPostResult
        {
            Success = false,
            StatusCode = statusCode,
            Message = message
        };
    }
}
