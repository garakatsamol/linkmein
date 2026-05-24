using LinkMeIn.Api.Contracts.Ai;
using System.Threading.Tasks;

namespace LinkMeIn.Api.Services
{
    public interface IPostSuggestionService
    {
        Task<GeneratePostSuggestionResponse> GeneratePostSuggestionAsync(GeneratePostSuggestionRequest request);
    }
}
