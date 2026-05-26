using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using LinkMeIn.Api.Contracts.Ai;

namespace LinkMeIn.Api.Services
{
    public class OllamaPostSuggestionService : IPostSuggestionService
    {
        private readonly HttpClient _httpClient;
        private readonly string _endpoint;
        private readonly string _model;
        private readonly int _timeoutSeconds;
        private readonly ILogger<OllamaPostSuggestionService> _logger;

        public OllamaPostSuggestionService(IConfiguration config, ILogger<OllamaPostSuggestionService> logger)
        {
            _endpoint = config["Ai:OllamaEndpoint"] ?? "http://localhost:11434";
            _model = config["Ai:Model"] ?? "llama3.1:8b";
            if (!int.TryParse(config["Ai:TimeoutSeconds"], out _timeoutSeconds))
                _timeoutSeconds = 120;
            _httpClient = new HttpClient();
            _logger = logger;
        }

        public async Task<GeneratePostSuggestionResponse> GeneratePostSuggestionAsync(GeneratePostSuggestionRequest request)
        {
            var ollamaRequest = new OllamaGenerateRequest
            {
                Model = _model,
                Prompt = BuildPrompt(request),
                Stream = false,
                NumPredict = 300 // conservative output limit
            };

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_timeoutSeconds));
                var response = await _httpClient.PostAsJsonAsync($"{_endpoint}/api/generate", ollamaRequest, cts.Token);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Ollama returned non-success: {StatusCode}", response.StatusCode);
                    return new GeneratePostSuggestionResponse { SuggestedText = string.Empty, Message = "Ollama AI provider unavailable (status code)." };
                }
                var ollamaResponse = await response.Content.ReadFromJsonAsync<OllamaGenerateResponse>(cancellationToken: cts.Token);
                if (ollamaResponse == null || string.IsNullOrWhiteSpace(ollamaResponse.Response))
                {
                    return new GeneratePostSuggestionResponse { SuggestedText = string.Empty, Message = "Ollama AI provider returned no suggestion." };
                }
                return new GeneratePostSuggestionResponse { SuggestedText = ollamaResponse.Response.Trim(), Message = null };
            }
            catch (OperationCanceledException)
            {
                return new GeneratePostSuggestionResponse { SuggestedText = string.Empty, Message = $"Ollama AI provider timed out after {_timeoutSeconds} seconds." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ollama AI provider failed");
                return new GeneratePostSuggestionResponse { SuggestedText = string.Empty, Message = "Ollama AI provider error." };
            }
        }

        private static string BuildPrompt(GeneratePostSuggestionRequest request)
        {
            var tone = string.IsNullOrWhiteSpace(request.Tone) ? "professional" : request.Tone;
            var idea = string.IsNullOrWhiteSpace(request.Idea) ? "a LinkedIn post" : request.Idea.Trim();
            return $@"You are an expert LinkedIn content writer.

Turn the following rough thoughts, bullets, fragments, or messy notes into ONE practical, human, professional LinkedIn post in English with a {tone} tone.

Raw notes:
{idea}

Instructions:
- Output ONLY the final LinkedIn post text.
- Do not include a title.
- Do not include explanations, prefaces, labels, or phrases like 'Here is a post'.
- Do NOT include fake statistics, surveys, company names, links, or placeholders like [Your Company].
- Keep it around 150 words or less.
- Use short paragraphs.
- Keep the user's original angle and intent.
- Make it useful and specific without inventing facts.
- Add 2-4 relevant hashtags ONLY if they fit naturally at the end.";
        }

        private class OllamaGenerateRequest
        {
            [JsonPropertyName("model")]
            public string Model { get; set; } = string.Empty;
            [JsonPropertyName("prompt")]
            public string Prompt { get; set; } = string.Empty;
            [JsonPropertyName("stream")]
            public bool Stream { get; set; } = false;
            [JsonPropertyName("num_predict")]
            public int NumPredict { get; set; } = 300;
        }

        private class OllamaGenerateResponse
        {
            [JsonPropertyName("response")]
            public string Response { get; set; } = string.Empty;
        }
    }
}
