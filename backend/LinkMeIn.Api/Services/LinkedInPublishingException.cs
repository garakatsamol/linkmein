namespace LinkMeIn.Api.Services;

public class LinkedInPublishingException(string safeMessage) : Exception(safeMessage)
{
    public string SafeMessage { get; } = safeMessage;
}
