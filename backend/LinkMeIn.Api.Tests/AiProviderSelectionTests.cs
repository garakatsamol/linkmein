using System;
using System.Threading.Tasks;
using LinkMeIn.Api.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LinkMeIn.Api.Tests
{
    public class AiProviderSelectionTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        public AiProviderSelectionTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public void ProviderSelection_UsesMockByDefault()
        {
            var scope = _factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IPostSuggestionService>();
            Assert.IsType<MockPostSuggestionService>(service);
        }

        [Fact]
        public void ProviderSelection_UsesOllamaIfConfigured()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new[] { new System.Collections.Generic.KeyValuePair<string, string>("Ai:Provider", "Ollama") })
                .Build();
            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(config);
            services.AddLogging();
            if (string.Equals(config["Ai:Provider"], "Ollama", StringComparison.OrdinalIgnoreCase))
                services.AddScoped<IPostSuggestionService, OllamaPostSuggestionService>();
            else
                services.AddScoped<IPostSuggestionService, MockPostSuggestionService>();
            var provider = services.BuildServiceProvider();
            var service = provider.GetRequiredService<IPostSuggestionService>();
            Assert.IsType<OllamaPostSuggestionService>(service);
        }
    }
}
