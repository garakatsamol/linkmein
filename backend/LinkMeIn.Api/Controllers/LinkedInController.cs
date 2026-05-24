using System.Security.Cryptography;
using LinkMeIn.Api.Contracts.LinkedIn;
using LinkMeIn.Api.Data;
using LinkMeIn.Api.Entities;
using LinkMeIn.Api.Options;
using LinkMeIn.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LinkMeIn.Api.Controllers;

[ApiController]
[Route("api/linkedin")]
public class LinkedInController : ControllerBase
{
    private const string DefaultOwnerId = "default-owner";
    private static readonly TimeSpan StateLifetime = TimeSpan.FromMinutes(10);

    private readonly LinkMeInDbContext _db;
    private readonly LinkedInOptions _linkedInOptions;
    private readonly ILinkedInOAuthClient _linkedInOAuthClient;
    private readonly ITokenEncryptionService _tokenEncryption;

    public LinkedInController(
        LinkMeInDbContext db,
        IOptions<LinkedInOptions> linkedInOptions,
        ILinkedInOAuthClient linkedInOAuthClient,
        ITokenEncryptionService tokenEncryption)
    {
        _db = db;
        _linkedInOptions = linkedInOptions.Value;
        _linkedInOAuthClient = linkedInOAuthClient;
        _tokenEncryption = tokenEncryption;
    }

    [HttpGet("status")]
    public async Task<ActionResult<LinkedInStatusResponse>> GetStatus(CancellationToken cancellationToken)
    {
        var connection = await _db.LinkedInConnections
            .AsNoTracking()
            .Where(item => item.OwnerId == DefaultOwnerId && item.DisconnectedAt == null)
            .OrderByDescending(item => item.ConnectedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (connection == null)
        {
            return Ok(new LinkedInStatusResponse { Connected = false });
        }

        return Ok(new LinkedInStatusResponse
        {
            Connected = true,
            DisplayName = connection.DisplayName,
            Email = null,
            ConnectedAt = connection.ConnectedAt,
            AccessTokenExpiresAt = connection.AccessTokenExpiresAt,
            Scopes = SplitScopes(connection.Scopes)
        });
    }

    [HttpGet("oauth/start")]
    public async Task<IActionResult> StartOAuth(CancellationToken cancellationToken)
    {
        if (!IsConfigured())
        {
            return BadRequest("LinkedIn OAuth is not configured.");
        }

        var now = DateTimeOffset.UtcNow;
        var state = CreateSecureState();
        _db.OAuthStates.Add(new OAuthState
        {
            Id = Guid.NewGuid(),
            OwnerId = DefaultOwnerId,
            State = state,
            CreatedAt = now,
            ExpiresAt = now.Add(StateLifetime)
        });
        await _db.SaveChangesAsync(cancellationToken);

        var authorizationUrl = QueryHelpers.AddQueryString(
            _linkedInOptions.AuthorizationEndpoint,
            new Dictionary<string, string?>
            {
                ["response_type"] = "code",
                ["client_id"] = _linkedInOptions.ClientId,
                ["redirect_uri"] = _linkedInOptions.RedirectUri,
                ["scope"] = string.Join(" ", _linkedInOptions.Scopes),
                ["state"] = state
            });

        return Redirect(authorizationUrl);
    }

    [HttpGet("oauth/callback")]
    public async Task<IActionResult> OAuthCallback(
        [FromQuery] string? code,
        [FromQuery] string? state,
        [FromQuery] string? error,
        [FromQuery(Name = "error_description")] string? errorDescription,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(error))
        {
            return BadRequest(new { message = "LinkedIn OAuth returned an error.", error, errorDescription });
        }

        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(state))
        {
            return BadRequest("OAuth callback requires code and state.");
        }

        var now = DateTimeOffset.UtcNow;
        var oauthState = await _db.OAuthStates
            .FirstOrDefaultAsync(
                item =>
                    item.OwnerId == DefaultOwnerId &&
                    item.State == state &&
                    item.ConsumedAt == null &&
                    item.ExpiresAt > now,
                cancellationToken);

        if (oauthState == null)
        {
            return BadRequest("OAuth state is invalid or expired.");
        }

        LinkedInTokenResponse tokenResponse;
        try
        {
            tokenResponse = await _linkedInOAuthClient.ExchangeCodeForTokenAsync(code, _linkedInOptions, cancellationToken);
        }
        catch (Exception)
        {
            return Problem("LinkedIn token exchange failed.");
        }

        var scopes = string.IsNullOrWhiteSpace(tokenResponse.Scope)
            ? string.Join(" ", _linkedInOptions.Scopes)
            : tokenResponse.Scope;

        var existingConnections = await _db.LinkedInConnections
            .Where(item => item.OwnerId == DefaultOwnerId && item.DisconnectedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var existingConnection in existingConnections)
        {
            existingConnection.DisconnectedAt = now;
        }

        _db.LinkedInConnections.Add(new LinkedInConnection
        {
            Id = Guid.NewGuid(),
            OwnerId = DefaultOwnerId,
            AccessTokenEncrypted = _tokenEncryption.Protect(tokenResponse.AccessToken),
            RefreshTokenEncrypted = string.IsNullOrWhiteSpace(tokenResponse.RefreshToken)
                ? null
                : _tokenEncryption.Protect(tokenResponse.RefreshToken),
            AccessTokenExpiresAt = now.AddSeconds(Math.Max(tokenResponse.ExpiresIn, 0)),
            Scopes = scopes,
            ConnectedAt = now
        });
        oauthState.ConsumedAt = now;

        await _db.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            connected = true,
            message = "LinkedIn connection saved server-side. No tokens were returned to the browser."
        });
    }

    [HttpPost("disconnect")]
    public async Task<IActionResult> Disconnect(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var connections = await _db.LinkedInConnections
            .Where(item => item.OwnerId == DefaultOwnerId && item.DisconnectedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var connection in connections)
        {
            connection.DisconnectedAt = now;
        }

        await _db.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private bool IsConfigured()
    {
        return !string.IsNullOrWhiteSpace(_linkedInOptions.ClientId) &&
            !string.IsNullOrWhiteSpace(_linkedInOptions.ClientSecret) &&
            !string.IsNullOrWhiteSpace(_linkedInOptions.RedirectUri) &&
            !string.IsNullOrWhiteSpace(_linkedInOptions.AuthorizationEndpoint) &&
            !string.IsNullOrWhiteSpace(_linkedInOptions.TokenEndpoint) &&
            _linkedInOptions.Scopes.Count > 0;
    }

    private static string CreateSecureState()
    {
        Span<byte> bytes = stackalloc byte[32];
        RandomNumberGenerator.Fill(bytes);
        return WebEncoders.Base64UrlEncode(bytes);
    }

    private static IReadOnlyList<string> SplitScopes(string scopes)
    {
        return scopes.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}
