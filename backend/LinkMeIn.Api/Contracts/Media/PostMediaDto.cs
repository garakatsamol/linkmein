using System;

namespace LinkMeIn.Api.Contracts.Media
{
    public class PostMediaDto
    {
        public Guid Id { get; set; }
        public Guid PostId { get; set; }
        public string FileName { get; set; } = default!;
        public string ContentType { get; set; } = default!;
        public long SizeBytes { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string? LinkedInAssetUrn { get; set; }
    }
}
