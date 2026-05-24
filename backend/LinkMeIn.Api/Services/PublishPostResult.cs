namespace LinkMeIn.Api.Services;

public class PublishPostResult
{
    public bool Success { get; set; }
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? LinkedInPostId { get; set; }
}
