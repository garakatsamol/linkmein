namespace LinkMeIn.Api.Contracts.LinkedIn;

public class LinkedInStatusResponse
{
    public bool Connected { get; set; }
    public string? DisplayName { get; set; }
    public string? Email { get; set; }
    public DateTimeOffset? ConnectedAt { get; set; }
    public DateTimeOffset? AccessTokenExpiresAt { get; set; }
    public IReadOnlyList<string> Scopes { get; set; } = [];
}
