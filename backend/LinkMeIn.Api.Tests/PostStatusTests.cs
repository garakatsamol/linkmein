using LinkMeIn.Api.Entities;

namespace LinkMeIn.Api.Tests;

public class PostStatusTests
{
    [Fact]
    public void PublishingStatus_IsDefinedForBackendWorkflow()
    {
        Assert.Contains(PostStatus.Publishing, Enum.GetValues<PostStatus>());
    }
}
