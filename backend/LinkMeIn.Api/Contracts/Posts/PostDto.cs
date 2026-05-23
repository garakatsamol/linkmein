namespace LinkMeIn.Api.Contracts.Posts
{
    public class PostDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTimeOffset? ScheduledFor { get; set; }
        public DateTimeOffset? PublishedAt { get; set; }
        public string? LinkedInPostId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}