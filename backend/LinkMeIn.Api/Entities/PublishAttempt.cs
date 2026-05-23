namespace LinkMeIn.Api.Entities;

public class PublishAttempt
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public string OwnerId { get; set; } = "default-owner";
    public PublishAttemptStatus Status { get; set; } = PublishAttemptStatus.Pending;
    public DateTimeOffset AttemptedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public string? LinkedInPostId { get; set; }
    public string? RequestSummary { get; set; }
    public string? ResponseSummary { get; set; }

    public Post? Post { get; set; }
}
