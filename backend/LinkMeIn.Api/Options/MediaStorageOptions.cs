namespace LinkMeIn.Api.Options
{
    public class MediaStorageOptions
    {
        public string RootPath { get; set; } = string.Empty;
        public long MaxFileSizeBytes { get; set; }
        public int MaxImagesPerPost { get; set; }
        public string[] AllowedContentTypes { get; set; } = [];
    }
}
