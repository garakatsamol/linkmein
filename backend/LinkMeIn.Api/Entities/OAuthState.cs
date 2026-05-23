namespace LinkMeIn.Api.Entities;

public class OAuthState
{
    public Guid Id { get; set; }
    public string OwnerId { get; set; } = "default-owner";
    public string State { get; set; } = string.Empty;
    public string? CodeVerifierHash { get; set; }
    public string? ReturnUrl { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? ConsumedAt { get; set; }
}
