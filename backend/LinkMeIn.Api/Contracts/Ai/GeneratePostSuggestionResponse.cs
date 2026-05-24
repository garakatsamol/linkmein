namespace LinkMeIn.Api.Contracts.Ai
{
    public class GeneratePostSuggestionResponse
    {
        public string SuggestedText { get; set; } = string.Empty;
        public string? Message { get; set; }
    }
}
