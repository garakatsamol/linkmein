using LinkMeIn.Api.Contracts.Ai;
using Microsoft.AspNetCore.Mvc;

namespace LinkMeIn.Api.Controllers
{
    [ApiController]
    [Route("api/ai/post-suggestions")]
    public class AiController : ControllerBase
    {
        [HttpPost]
        public ActionResult<GeneratePostSuggestionResponse> GeneratePostSuggestion([FromBody] GeneratePostSuggestionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Idea))
            {
                return BadRequest(new GeneratePostSuggestionResponse
                {
                    SuggestedText = string.Empty,
                    Message = "Idea is required."
                });
            }

            var isGreek = (request.Language?.Trim().ToLowerInvariant() == "el" || request.Language?.Trim().ToLowerInvariant() == "greek");
            string suggestion;
            if (isGreek)
            {
                suggestion = $"Ιδέα: {request.Idea}\n\nΑκολουθεί μια πρόταση ανάρτησης με τόνο '{request.Tone ?? "ουδέτερο"}':\nΑυτή είναι μια προτεινόμενη ανάρτηση για το LinkedIn βασισμένη στην ιδέα σας.";
            }
            else
            {
                suggestion = $"Idea: {request.Idea}\n\nHere is a suggested LinkedIn post in a '{request.Tone ?? "neutral"}' tone:\nThis is a suggested post for LinkedIn based on your idea.";
            }

            return Ok(new GeneratePostSuggestionResponse
            {
                SuggestedText = suggestion,
                Message = null
            });
        }
    }
}
