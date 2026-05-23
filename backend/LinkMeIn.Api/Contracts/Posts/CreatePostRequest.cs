namespace LinkMeIn.Api.Contracts.Posts
{
    public class CreatePostRequest
    {
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public DateTimeOffset? ScheduledFor { get; set; }
    }
}