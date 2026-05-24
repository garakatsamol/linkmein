using System.Net;
using System.Text;
using System.Text.Json;
using LinkMeIn.Api.Data;
using LinkMeIn.Api.Entities;
using LinkMeIn.Api.Options;
using LinkMeIn.Api.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LinkMeIn.Api.Tests;

public class LinkedInOAuthControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public LinkedInOAuthControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task OAuthCallback_IdTokenSub_StoresLinkedInMemberUrn()
    {
        var factory = CreateOAuthFactory(new FakeLinkedInOAuthClient
        {
            TokenResponse = CreateTokenResponse(CreateIdToken("abc123"))
        });
        await ResetDatabaseAsync(factory);
        var state = await AddOAuthStateAsync(factory);
        var client = factory.CreateClient();

        var response = await client.GetAsync($"/api/linkedin/oauth/callback?code=test-code&state={state}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LinkMeInDbContext>();
        var connection = await db.LinkedInConnections.SingleAsync();
        Assert.Equal("urn:li:person:abc123", connection.LinkedInMemberId);
        Assert.DoesNotContain("token", connection.AccessTokenEncrypted, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task OAuthCallback_NonPersonUrnSub_DoesNotStoreMemberUrn()
    {
        var factory = CreateOAuthFactory(new FakeLinkedInOAuthClient
        {
            TokenResponse = CreateTokenResponse(CreateIdToken("urn:li:organization:123"))
        });
        await ResetDatabaseAsync(factory);
        var state = await AddOAuthStateAsync(factory);
        var client = factory.CreateClient();

        var response = await client.GetAsync($"/api/linkedin/oauth/callback?code=test-code&state={state}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LinkMeInDbContext>();
        var connection = await db.LinkedInConnections.SingleAsync();
        Assert.Null(connection.LinkedInMemberId);
    }

    private WebApplicationFactory<Program> CreateOAuthFactory(FakeLinkedInOAuthClient fakeLinkedInOAuthClient)
    {
        return _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ILinkedInOAuthClient>();
                services.RemoveAll<ITokenEncryptionService>();
                services.AddSingleton<ILinkedInOAuthClient>(fakeLinkedInOAuthClient);
                services.AddSingleton<ITokenEncryptionService, PrefixTokenEncryptionService>();
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

    private static async Task<string> AddOAuthStateAsync(WebApplicationFactory<Program> factory)
    {
        const string state = "test-state";
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LinkMeInDbContext>();
        db.OAuthStates.Add(new OAuthState
        {
            Id = Guid.NewGuid(),
            OwnerId = "default-owner",
            State = state,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10)
        });
        await db.SaveChangesAsync();
        return state;
    }

    private static LinkedInTokenResponse CreateTokenResponse(string idToken)
    {
        return new LinkedInTokenResponse
        {
            AccessToken = "fake-access-token",
            ExpiresIn = 3600,
            IdToken = idToken,
            Scope = "openid profile email w_member_social"
        };
    }

    private static string CreateIdToken(string subject)
    {
        var header = Base64UrlTextEncoder.Encode(Encoding.UTF8.GetBytes("{\"alg\":\"none\"}"));
        var payload = Base64UrlTextEncoder.Encode(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { sub = subject })));
        return $"{header}.{payload}.signature";
    }

    private class FakeLinkedInOAuthClient : ILinkedInOAuthClient
    {
        public LinkedInTokenResponse TokenResponse { get; set; } = new();

        public Task<LinkedInTokenResponse> ExchangeCodeForTokenAsync(
            string code,
            LinkedInOptions options,
            CancellationToken cancellationToken = default)
        {
            Assert.Equal("test-code", code);
            return Task.FromResult(TokenResponse);
        }
    }

    private class PrefixTokenEncryptionService : ITokenEncryptionService
    {
        public string Protect(string value) => $"protected:{value.Length}";

        public string Unprotect(string protectedValue) => protectedValue;
    }
}
