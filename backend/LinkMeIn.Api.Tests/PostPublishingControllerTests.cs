using System.Net;
using System.Net.Http.Json;
using LinkMeIn.Api.Contracts.Posts;
using LinkMeIn.Api.Contracts.Publishing;
using LinkMeIn.Api.Data;
using LinkMeIn.Api.Entities;
using LinkMeIn.Api.Options;
using LinkMeIn.Api.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LinkMeIn.Api.Tests;

public class PostPublishingControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public PostPublishingControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task PublishPost_NoLinkedInConnection_Returns400AndRecordsFailure()
    {
        await _factory.ResetDatabaseAsync();
        var client = _factory.CreateClient();
        var post = await CreatePostAsync(client);

        var response = await client.PostAsync($"/api/posts/{post.Id}/publish", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PublishPostResponse>();
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal("No active LinkedIn connection found.", result.Message);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LinkMeInDbContext>();
        var updatedPost = await db.Posts.FirstAsync(item => item.Id == post.Id);
        Assert.Equal(PostStatus.Failed, updatedPost.Status);
        Assert.Equal(PublishAttemptStatus.Failed, await db.PublishAttempts.Where(item => item.PostId == post.Id).Select(item => item.Status).SingleAsync());
    }

    [Fact]
    public async Task PublishPost_NoMemberUrn_Returns400WithReconnectMessage()
    {
        var factory = CreatePublishingFactory(new FakeLinkedInPublishingClient());
        await ResetDatabaseAsync(factory);
        var client = factory.CreateClient();
        var post = await CreatePostAsync(client);
        await AddConnectionAsync(factory, linkedInMemberUrn: null);

        var response = await client.PostAsync($"/api/posts/{post.Id}/publish", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PublishPostResponse>();
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal("LinkedIn member URN is missing. Reconnect LinkedIn before publishing.", result.Message);
    }

    [Fact]
    public async Task PublishPost_SuccessfulPublish_UpdatesPostAndAttempt()
    {
        var fakeLinkedInClient = new FakeLinkedInPublishingClient
        {
            Response = new LinkedInPublishResponse
            {
                LinkedInPostId = "urn:li:share:12345",
                ResponseSummary = "201 Created"
            }
        };
        var factory = CreatePublishingFactory(fakeLinkedInClient);
        await ResetDatabaseAsync(factory);
        var client = factory.CreateClient();
        var post = await CreatePostAsync(client);
        await AddConnectionAsync(factory, "urn:li:person:test-member");

        var response = await client.PostAsync($"/api/posts/{post.Id}/publish", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PublishPostResponse>();
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("urn:li:share:12345", result.LinkedInPostId);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LinkMeInDbContext>();
        var updatedPost = await db.Posts.FirstAsync(item => item.Id == post.Id);
        Assert.Equal(PostStatus.Published, updatedPost.Status);
        Assert.Equal("urn:li:share:12345", updatedPost.LinkedInPostId);
        Assert.NotNull(updatedPost.PublishedAt);

        var attempt = await db.PublishAttempts.SingleAsync(item => item.PostId == post.Id);
        Assert.Equal(PublishAttemptStatus.Succeeded, attempt.Status);
        Assert.Equal("urn:li:share:12345", attempt.LinkedInPostId);
        Assert.Equal("Test post content", fakeLinkedInClient.Commentary);
        Assert.Equal("urn:li:person:test-member", fakeLinkedInClient.AuthorUrn);
    }

    [Fact]
    public async Task PublishPost_WithSingleImage_UploadsImageAndPublishesImagePost()
    {
        var fakeLinkedInClient = new FakeLinkedInPublishingClient
        {
            ImageUploadResponse = new LinkedInImageUploadResponse
            {
                ImageUrn = "urn:li:image:test-image",
                ResponseSummary = "201 Created"
            },
            Response = new LinkedInPublishResponse
            {
                LinkedInPostId = "urn:li:share:image-post",
                ResponseSummary = "201 Created"
            }
        };
        var fakeMediaStorage = new FakeMediaStorageService();
        fakeMediaStorage.AddFile("posts/test-image.png", [1, 2, 3, 4]);
        var factory = CreatePublishingFactory(fakeLinkedInClient, fakeMediaStorage);
        await ResetDatabaseAsync(factory);
        var client = factory.CreateClient();
        var post = await CreatePostAsync(client);
        await AddConnectionAsync(factory, "urn:li:person:test-member");
        var mediaId = await AddMediaAsync(factory, post.Id, "posts/test-image.png");

        var response = await client.PostAsync($"/api/posts/{post.Id}/publish", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PublishPostResponse>();
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("urn:li:share:image-post", result.LinkedInPostId);
        Assert.Equal(1, fakeLinkedInClient.UploadImageCalls);
        Assert.Equal(1, fakeLinkedInClient.SingleImagePublishCalls);
        Assert.Equal(0, fakeLinkedInClient.TextPublishCalls);
        Assert.Equal("urn:li:image:test-image", fakeLinkedInClient.PublishedImageUrn);
        Assert.Equal("image/png", fakeLinkedInClient.UploadedContentType);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LinkMeInDbContext>();
        var updatedMedia = await db.PostMedia.FirstAsync(item => item.Id == mediaId);
        Assert.Equal("urn:li:image:test-image", updatedMedia.LinkedInAssetUrn);
        var updatedPost = await db.Posts.FirstAsync(item => item.Id == post.Id);
        Assert.Equal(PostStatus.Published, updatedPost.Status);
        Assert.Equal("urn:li:share:image-post", updatedPost.LinkedInPostId);
        var attempt = await db.PublishAttempts.SingleAsync(item => item.PostId == post.Id);
        Assert.Equal(PublishAttemptStatus.Succeeded, attempt.Status);
    }

    [Fact]
    public async Task PublishPost_WithMissingLocalMediaFile_Returns400AndDoesNotCallLinkedIn()
    {
        var fakeLinkedInClient = new FakeLinkedInPublishingClient();
        var factory = CreatePublishingFactory(fakeLinkedInClient, new FakeMediaStorageService());
        await ResetDatabaseAsync(factory);
        var client = factory.CreateClient();
        var post = await CreatePostAsync(client);
        await AddConnectionAsync(factory, "urn:li:person:test-member");
        await AddMediaAsync(factory, post.Id, "posts/missing.png");

        var response = await client.PostAsync($"/api/posts/{post.Id}/publish", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PublishPostResponse>();
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal("Local media file is missing. Re-upload the image before publishing.", result.Message);
        Assert.Equal(0, fakeLinkedInClient.UploadImageCalls);
        Assert.Equal(0, fakeLinkedInClient.SingleImagePublishCalls);
        Assert.Equal(0, fakeLinkedInClient.TextPublishCalls);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LinkMeInDbContext>();
        var updatedPost = await db.Posts.FirstAsync(item => item.Id == post.Id);
        Assert.Equal(PostStatus.Failed, updatedPost.Status);
        var attempt = await db.PublishAttempts.SingleAsync(item => item.PostId == post.Id);
        Assert.Equal(PublishAttemptStatus.Failed, attempt.Status);
        Assert.Equal("Local media file is missing. Re-upload the image before publishing.", attempt.ErrorMessage);
    }

    [Fact]
    public async Task PublishPost_WithMultipleImages_Returns400AndDoesNotCallLinkedIn()
    {
        var fakeLinkedInClient = new FakeLinkedInPublishingClient();
        var fakeMediaStorage = new FakeMediaStorageService();
        fakeMediaStorage.AddFile("posts/first.png", [1, 2, 3, 4]);
        fakeMediaStorage.AddFile("posts/second.png", [5, 6, 7, 8]);
        var factory = CreatePublishingFactory(fakeLinkedInClient, fakeMediaStorage);
        await ResetDatabaseAsync(factory);
        var client = factory.CreateClient();
        var post = await CreatePostAsync(client);
        await AddConnectionAsync(factory, "urn:li:person:test-member");
        await AddMediaAsync(factory, post.Id, "posts/first.png");
        await AddMediaAsync(factory, post.Id, "posts/second.png");

        var response = await client.PostAsync($"/api/posts/{post.Id}/publish", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PublishPostResponse>();
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal("Publishing multiple images is not supported yet.", result.Message);
        Assert.Equal(0, fakeLinkedInClient.UploadImageCalls);
        Assert.Equal(0, fakeLinkedInClient.SingleImagePublishCalls);
        Assert.Equal(0, fakeLinkedInClient.TextPublishCalls);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LinkMeInDbContext>();
        var updatedPost = await db.Posts.FirstAsync(item => item.Id == post.Id);
        Assert.Equal(PostStatus.Failed, updatedPost.Status);
        var attempt = await db.PublishAttempts.SingleAsync(item => item.PostId == post.Id);
        Assert.Equal(PublishAttemptStatus.Failed, attempt.Status);
        Assert.Equal("Publishing multiple images is not supported yet.", attempt.ErrorMessage);
    }

    private WebApplicationFactory<Program> CreatePublishingFactory(
        FakeLinkedInPublishingClient fakeLinkedInClient,
        FakeMediaStorageService? fakeMediaStorage = null)
    {
        return _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ILinkedInPublishingClient>();
                services.RemoveAll<IMediaStorageService>();
                services.RemoveAll<ITokenEncryptionService>();
                services.AddSingleton<ILinkedInPublishingClient>(fakeLinkedInClient);
                services.AddSingleton<IMediaStorageService>(fakeMediaStorage ?? new FakeMediaStorageService());
                services.AddSingleton<ITokenEncryptionService, NoOpTokenEncryptionService>();
            });
        });
    }

    private static async Task ResetDatabaseAsync(WebApplicationFactory<Program> factory)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LinkMeInDbContext>();
        db.PostMedia.RemoveRange(db.PostMedia);
        db.PublishAttempts.RemoveRange(db.PublishAttempts);
        db.Posts.RemoveRange(db.Posts);
        db.LinkedInConnections.RemoveRange(db.LinkedInConnections);
        db.OAuthStates.RemoveRange(db.OAuthStates);
        await db.SaveChangesAsync();
    }

    private static async Task<Guid> AddMediaAsync(WebApplicationFactory<Program> factory, Guid postId, string storagePath)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LinkMeInDbContext>();
        var media = new PostMedia
        {
            Id = Guid.NewGuid(),
            PostId = postId,
            OwnerId = "default-owner",
            FileName = Path.GetFileName(storagePath),
            ContentType = "image/png",
            SizeBytes = 4,
            StoragePath = storagePath,
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.PostMedia.Add(media);
        await db.SaveChangesAsync();
        return media.Id;
    }

    private static async Task<PostDto> CreatePostAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/posts", new CreatePostRequest
        {
            Title = "Publish me",
            Content = "Test post content"
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var post = await response.Content.ReadFromJsonAsync<PostDto>();
        Assert.NotNull(post);
        return post;
    }

    private static async Task AddConnectionAsync(WebApplicationFactory<Program> factory, string? linkedInMemberUrn)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LinkMeInDbContext>();
        db.LinkedInConnections.Add(new LinkedInConnection
        {
            Id = Guid.NewGuid(),
            OwnerId = "default-owner",
            AccessTokenEncrypted = "access-token",
            LinkedInMemberId = linkedInMemberUrn,
            AccessTokenExpiresAt = DateTimeOffset.UtcNow.AddHours(1),
            Scopes = "openid profile email w_member_social",
            ConnectedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();
    }

    private class FakeLinkedInPublishingClient : ILinkedInPublishingClient
    {
        public LinkedInPublishResponse Response { get; set; } = new();
        public LinkedInImageUploadResponse ImageUploadResponse { get; set; } = new();
        public string? AuthorUrn { get; private set; }
        public string? Commentary { get; private set; }
        public string? PublishedImageUrn { get; private set; }
        public string? UploadedContentType { get; private set; }
        public string? UploadedFileName { get; private set; }
        public int TextPublishCalls { get; private set; }
        public int UploadImageCalls { get; private set; }
        public int SingleImagePublishCalls { get; private set; }

        public Task<LinkedInPublishResponse> PublishTextPostAsync(
            string accessToken,
            string authorUrn,
            string commentary,
            LinkedInOptions options,
            CancellationToken cancellationToken = default)
        {
            Assert.Equal("access-token", accessToken);
            Assert.Equal("202605", options.ApiVersion);
            TextPublishCalls++;
            AuthorUrn = authorUrn;
            Commentary = commentary;
            return Task.FromResult(Response);
        }

        public Task<LinkedInImageUploadResponse> UploadImageAsync(
            string accessToken,
            string ownerUrn,
            Stream imageContent,
            string contentType,
            string fileName,
            LinkedInOptions options,
            CancellationToken cancellationToken = default)
        {
            Assert.Equal("access-token", accessToken);
            Assert.Equal("urn:li:person:test-member", ownerUrn);
            Assert.Equal("202605", options.ApiVersion);
            UploadImageCalls++;
            UploadedContentType = contentType;
            UploadedFileName = fileName;
            Assert.True(imageContent.Length > 0);
            return Task.FromResult(ImageUploadResponse);
        }

        public Task<LinkedInPublishResponse> PublishSingleImagePostAsync(
            string accessToken,
            string authorUrn,
            string commentary,
            string imageUrn,
            LinkedInOptions options,
            CancellationToken cancellationToken = default)
        {
            Assert.Equal("access-token", accessToken);
            Assert.Equal("202605", options.ApiVersion);
            SingleImagePublishCalls++;
            AuthorUrn = authorUrn;
            Commentary = commentary;
            PublishedImageUrn = imageUrn;
            return Task.FromResult(Response);
        }
    }

    private class FakeMediaStorageService : IMediaStorageService
    {
        private readonly Dictionary<string, byte[]> _files = [];

        public void AddFile(string storagePath, byte[] content)
        {
            _files[storagePath] = content;
        }

        public Task<string> SaveFileAsync(string postId, string fileName, Stream fileStream, string contentType)
        {
            throw new NotSupportedException("Publishing tests do not save media files.");
        }

        public Task<Stream?> OpenReadAsync(string storagePath, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Stream?>(
                _files.TryGetValue(storagePath, out var content)
                    ? new MemoryStream(content)
                    : null);
        }

        public Task DeleteFileAsync(string storagePath)
        {
            throw new NotSupportedException("Publishing tests do not delete media files.");
        }
    }

    private class NoOpTokenEncryptionService : ITokenEncryptionService
    {
        public string Protect(string value) => value;

        public string Unprotect(string protectedValue) => protectedValue;
    }
}
