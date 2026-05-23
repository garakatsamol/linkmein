using System.ComponentModel.DataAnnotations;

namespace LinkMeIn.Api.Entities;

public class Post
{
    public Guid Id { get; set; }
    public string OwnerId { get; set; } = "default-owner";
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public PostStatus Status { get; set; } = PostStatus.Draft;
    public DateTimeOffset? ScheduledFor { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
    public string? LinkedInPostId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; } = [];

    public ICollection<PostMedia> Media { get; set; } = [];
    public ICollection<PublishAttempt> PublishAttempts { get; set; } = [];
}
