namespace LinkMeIn.Api.Entities;

public class PostMedia
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public string OwnerId { get; set; } = "default-owner";
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string StoragePath { get; set; } = string.Empty;
    public string? LinkedInAssetUrn { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public Post? Post { get; set; }
}
