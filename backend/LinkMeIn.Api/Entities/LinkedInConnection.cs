namespace LinkMeIn.Api.Entities;

public class LinkedInConnection
{
    public Guid Id { get; set; }
    public string OwnerId { get; set; } = "default-owner";
    public string? LinkedInMemberId { get; set; }
    public string? DisplayName { get; set; }
    public string AccessTokenEncrypted { get; set; } = string.Empty;
    public string? RefreshTokenEncrypted { get; set; }
    public DateTimeOffset AccessTokenExpiresAt { get; set; }
    public string Scopes { get; set; } = string.Empty;
    public DateTimeOffset ConnectedAt { get; set; }
    public DateTimeOffset? DisconnectedAt { get; set; }
}
