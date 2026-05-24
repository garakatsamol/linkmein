namespace LinkMeIn.Api.Options;

public class LinkedInOptions
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
    public string AuthorizationEndpoint { get; set; } = "https://www.linkedin.com/oauth/v2/authorization";
    public string TokenEndpoint { get; set; } = "https://www.linkedin.com/oauth/v2/accessToken";
    public List<string> Scopes { get; set; } = [];
}
