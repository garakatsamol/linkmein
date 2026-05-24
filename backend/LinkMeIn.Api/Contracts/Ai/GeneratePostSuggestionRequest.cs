namespace LinkMeIn.Api.Contracts.Ai
{
    public class GeneratePostSuggestionRequest
    {
        public string Idea { get; set; } = string.Empty;
        public string? Tone { get; set; }
        public string? Language { get; set; }
    }
}
