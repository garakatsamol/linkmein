using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using LinkMeIn.Api.Contracts.Media;
using LinkMeIn.Api.Contracts.Posts;
using Xunit;

namespace LinkMeIn.Api.Tests
{
    public class MediaControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        public MediaControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        private HttpClient CreateClient() => _factory.CreateClient();

        [Fact]
        public async Task GetMedia_ForNewPost_ReturnsEmptyArray()
        {
            await _factory.ResetDatabaseAsync();
            var client = CreateClient();
            // Create a post
            var createRequest = new CreatePostRequest
            {
                Title = "Media Test Post",
                Content = "No media yet"
            };
            var createResp = await client.PostAsJsonAsync("/api/posts", createRequest);
            Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
            var createdPost = await createResp.Content.ReadFromJsonAsync<PostDto>();
            Assert.NotNull(createdPost);

            // Get media for the post
            var getResp = await client.GetAsync($"/api/posts/{createdPost.Id}/media");
            Assert.Equal(HttpStatusCode.OK, getResp.StatusCode);
            var media = await getResp.Content.ReadFromJsonAsync<List<PostMediaDto>>();
            Assert.NotNull(media);
            Assert.Empty(media);
        }
    }
}
