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

    private WebApplicationFactory<Program> CreatePublishingFactory(FakeLinkedInPublishingClient fakeLinkedInClient)
    {
        return _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ILinkedInPublishingClient>();
                services.RemoveAll<ITokenEncryptionService>();
                services.AddSingleton<ILinkedInPublishingClient>(fakeLinkedInClient);
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
        public string? AuthorUrn { get; private set; }
        public string? Commentary { get; private set; }

        public Task<LinkedInPublishResponse> PublishTextPostAsync(
            string accessToken,
            string authorUrn,
            string commentary,
            LinkedInOptions options,
            CancellationToken cancellationToken = default)
        {
            Assert.Equal("access-token", accessToken);
            Assert.Equal("202605", options.ApiVersion);
            AuthorUrn = authorUrn;
            Commentary = commentary;
            return Task.FromResult(Response);
        }
    }

    private class NoOpTokenEncryptionService : ITokenEncryptionService
    {
        public string Protect(string value) => value;

        public string Unprotect(string protectedValue) => protectedValue;
    }
}
