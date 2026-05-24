using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using LinkMeIn.Api.Contracts.Ai;
using Xunit;

namespace LinkMeIn.Api.Tests
{
    public class AiControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        public AiControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GeneratePostSuggestion_ValidRequest_Returns200AndSuggestion()
        {
            var client = _factory.CreateClient();
            var request = new GeneratePostSuggestionRequest
            {
                Idea = "How to use AI for productivity",
                Tone = "friendly",
                Language = "en"
            };
            var response = await client.PostAsJsonAsync("/api/ai/post-suggestions", request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<GeneratePostSuggestionResponse>();
            Assert.NotNull(result);
            Assert.Contains("How to use AI for productivity", result!.SuggestedText);
        }

        [Fact]
        public async Task GeneratePostSuggestion_MissingIdea_Returns400()
        {
            var client = _factory.CreateClient();
            var request = new GeneratePostSuggestionRequest
            {
                Idea = "",
                Tone = "friendly",
                Language = "en"
            };
            var response = await client.PostAsJsonAsync("/api/ai/post-suggestions", request);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<GeneratePostSuggestionResponse>();
            Assert.NotNull(result);
            Assert.Equal(string.Empty, result!.SuggestedText);
            Assert.Equal("Idea is required.", result.Message);
        }
    }
}
