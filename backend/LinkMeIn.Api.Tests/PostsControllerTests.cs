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
        [Fact]
        public async Task GetPostById_Returns200AndCorrectPost()
        {
            var client = CreateClient();
            var createRequest = new LinkMeIn.Api.Contracts.Posts.CreatePostRequest
            {
                Title = "GetById Title",
                Content = "GetById Content"
            };
            var createResp = await client.PostAsJsonAsync("/api/posts", createRequest);
            Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
            var createdPost = await createResp.Content.ReadFromJsonAsync<LinkMeIn.Api.Contracts.Posts.PostDto>();
            Assert.NotNull(createdPost);

            var getResp = await client.GetAsync($"/api/posts/{createdPost.Id}");
            Assert.Equal(HttpStatusCode.OK, getResp.StatusCode);
            var fetchedPost = await getResp.Content.ReadFromJsonAsync<LinkMeIn.Api.Contracts.Posts.PostDto>();
            Assert.NotNull(fetchedPost);
            Assert.Equal(createdPost.Id, fetchedPost.Id);
            Assert.Equal(createdPost.Title, fetchedPost.Title);
            Assert.Equal(createdPost.Content, fetchedPost.Content);
            Assert.Equal(createdPost.Status, fetchedPost.Status);
        }
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
        [Fact]
        public async Task CreatePost_ValidRequest_Returns201AndPostDto()
        {
            var client = CreateClient();
            var request = new LinkMeIn.Api.Contracts.Posts.CreatePostRequest
            {
                Title = "Test Title",
                Content = "Test Content"
            };
            var resp = await client.PostAsJsonAsync("/api/posts", request);
            Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
            var post = await resp.Content.ReadFromJsonAsync<LinkMeIn.Api.Contracts.Posts.PostDto>();
            Assert.NotNull(post);
            Assert.Equal(request.Title, post.Title);
            Assert.Equal(request.Content, post.Content);
            Assert.Equal("Draft", post.Status);
            Assert.NotEqual(Guid.Empty, post.Id);
        }

        [Fact]
        public async Task CreateScheduledPost_ValidRequest_Returns201AndScheduledPostDto()
        {
            var client = CreateClient();
            var futureDate = DateTimeOffset.UtcNow.AddDays(1);
            var request = new LinkMeIn.Api.Contracts.Posts.CreatePostRequest
            {
                Title = "Scheduled Title",
                Content = "Scheduled Content",
                ScheduledFor = futureDate
            };
            var resp = await client.PostAsJsonAsync("/api/posts", request);
            Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
            var post = await resp.Content.ReadFromJsonAsync<LinkMeIn.Api.Contracts.Posts.PostDto>();
            Assert.NotNull(post);
            Assert.Equal(request.Title, post.Title);
            Assert.Equal(request.Content, post.Content);
            Assert.Equal("Scheduled", post.Status);
            Assert.NotNull(post.ScheduledFor);
            Assert.NotEqual(Guid.Empty, post.Id);
        }
    }
}
