using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using LinkMeIn.Api.Contracts.Posts;
using LinkMeIn.Api.Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LinkMeIn.Api.Tests
{

    public class PostsControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;

        public PostsControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        private HttpClient CreateClient() => _factory.CreateClient();

        [Fact]
        public async Task GetAllPosts_EmptyDb_Returns200AndEmptyArray()
        {
            var client = CreateClient();
            var resp = await client.GetAsync("/api/posts");
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
            var posts = await resp.Content.ReadFromJsonAsync<List<PostDto>>();
            Assert.NotNull(posts);
            Assert.Empty(posts);
        }
    }
}
