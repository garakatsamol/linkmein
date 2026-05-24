
using LinkMeIn.Api.Contracts.Ai;
using LinkMeIn.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LinkMeIn.Api.Controllers
{
    [ApiController]
    [Route("api/ai/post-suggestions")]
    public class AiController : ControllerBase
    {
        private readonly IPostSuggestionService _postSuggestionService;

        public AiController(IPostSuggestionService postSuggestionService)
        {
            _postSuggestionService = postSuggestionService;
        }

        [HttpPost]
        public async Task<ActionResult<GeneratePostSuggestionResponse>> GeneratePostSuggestion([FromBody] GeneratePostSuggestionRequest request)
        {
            var response = await _postSuggestionService.GeneratePostSuggestionAsync(request);
            if (string.IsNullOrWhiteSpace(request.Idea))
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
    }
}
