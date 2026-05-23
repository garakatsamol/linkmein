using System.ComponentModel.DataAnnotations;

namespace LinkMeIn.Api.Contracts.Posts
{
    public class UpdatePostRequest
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = null!;

        [Required]
        public string Content { get; set; } = null!;

        public DateTimeOffset? ScheduledFor { get; set; }
    }
}