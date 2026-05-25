namespace LinkMeIn.Api.Contracts.Ai
{
    public class GeneratePostSuggestionResponse
    {
        public string? SuggestedTitle { get; set; }
        public string SuggestedText { get; set; } = string.Empty;
        public string? Message { get; set; }
    }
}
