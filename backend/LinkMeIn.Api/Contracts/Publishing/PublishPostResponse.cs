namespace LinkMeIn.Api.Contracts.Publishing;

public class PublishPostResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? LinkedInPostId { get; set; }
}
