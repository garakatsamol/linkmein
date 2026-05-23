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
    public class PostsControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private static readonly Guid DefaultOwnerId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        public PostsControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        private HttpClient CreateClient() => _factory.CreateClient();

        [Fact]
        public async Task CreateDraftPost_Succeeds()
        {
            var client = CreateClient();
            var req = new CreatePostRequest { Title = "Draft", Content = "Draft content" };
            var resp = await client.PostAsJsonAsync("/api/posts", req);
            Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
            var post = await resp.Content.ReadFromJsonAsync<PostDto>();
            Assert.NotNull(post);
            Assert.Equal("Draft", post!.Title);
            Assert.Equal("Draft content", post.Content);
            Assert.Equal("Draft", post.Status);
        }

        [Fact]
        public async Task CreateScheduledPost_SetsStatusScheduled()
        {
            var client = CreateClient();
            var future = DateTime.UtcNow.AddDays(1);
            var req = new CreatePostRequest { Title = "Scheduled", Content = "Scheduled content", ScheduledFor = future };
            var resp = await client.PostAsJsonAsync("/api/posts", req);
            Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
            var post = await resp.Content.ReadFromJsonAsync<PostDto>();
            Assert.NotNull(post);
            Assert.Equal("Scheduled", post!.Status);
            Assert.Equal(future.Date, post.ScheduledFor?.Date);
        }

        [Fact]
        public async Task GetAllPosts_ReturnsPosts()
        {
            var client = CreateClient();
            // Create a post
            await client.PostAsJsonAsync("/api/posts", new CreatePostRequest { Title = "A", Content = "B" });
            var resp = await client.GetAsync("/api/posts");
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
            var posts = await resp.Content.ReadFromJsonAsync<List<PostDto>>();
            Assert.NotNull(posts);
            Assert.NotEmpty(posts);
        }

        [Fact]
        public async Task GetPostById_ReturnsPost()
        {
            var client = CreateClient();
            var createResp = await client.PostAsJsonAsync("/api/posts", new CreatePostRequest { Title = "FindMe", Content = "FindContent" });
            var created = await createResp.Content.ReadFromJsonAsync<PostDto>();
            var resp = await client.GetAsync($"/api/posts/{created!.Id}");
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
            var post = await resp.Content.ReadFromJsonAsync<PostDto>();
            Assert.NotNull(post);
            Assert.Equal("FindMe", post!.Title);
        }

        [Fact]
        public async Task UpdatePost_ChangesFields()
        {
            var client = CreateClient();
            var createResp = await client.PostAsJsonAsync("/api/posts", new CreatePostRequest { Title = "Old", Content = "OldContent" });
            var created = await createResp.Content.ReadFromJsonAsync<PostDto>();
            var updateReq = new UpdatePostRequest { Title = "New", Content = "NewContent" };
            var updateResp = await client.PutAsJsonAsync($"/api/posts/{created!.Id}", updateReq);
            Assert.Equal(HttpStatusCode.OK, updateResp.StatusCode);
            var updated = await updateResp.Content.ReadFromJsonAsync<PostDto>();
            Assert.Equal("New", updated!.Title);
            Assert.Equal("NewContent", updated.Content);
        }

        [Fact]
        public async Task DeletePost_RemovesPost()
        {
            var client = CreateClient();
            var createResp = await client.PostAsJsonAsync("/api/posts", new CreatePostRequest { Title = "ToDelete", Content = "ToDeleteContent" });
            var created = await createResp.Content.ReadFromJsonAsync<PostDto>();
            var delResp = await client.DeleteAsync($"/api/posts/{created!.Id}");
            Assert.Equal(HttpStatusCode.NoContent, delResp.StatusCode);
            var getResp = await client.GetAsync($"/api/posts/{created.Id}");
            Assert.Equal(HttpStatusCode.NotFound, getResp.StatusCode);
        }

        [Fact]
        public async Task CreatePost_MissingTitleOrContent_Fails()
        {
            var client = CreateClient();
            var resp1 = await client.PostAsJsonAsync("/api/posts", new CreatePostRequest { Title = "", Content = "X" });
            Assert.Equal(HttpStatusCode.BadRequest, resp1.StatusCode);
            var resp2 = await client.PostAsJsonAsync("/api/posts", new CreatePostRequest { Title = "X", Content = "" });
            Assert.Equal(HttpStatusCode.BadRequest, resp2.StatusCode);
        }
    }
}
