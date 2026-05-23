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
            public async Task CreatePost_MissingContent_Returns400BadRequest()
            {
                await _factory.ResetDatabaseAsync();
                var client = CreateClient();
                var request = new LinkMeIn.Api.Contracts.Posts.CreatePostRequest
                {
                    Title = "Valid Title",
                    Content = ""
                };
                var resp = await client.PostAsJsonAsync("/api/posts", request);
                Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
            }
        [Fact]
        public async Task CreatePost_MissingTitle_Returns400BadRequest()
        {
            await _factory.ResetDatabaseAsync();
            var client = CreateClient();
            var request = new LinkMeIn.Api.Contracts.Posts.CreatePostRequest
            {
                Title = "",
                Content = "Valid Content"
            };
            var resp = await client.PostAsJsonAsync("/api/posts", request);
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }
        [Fact]
        public async Task DeletePost_ValidRequest_RemovesPost()
        {
            await _factory.ResetDatabaseAsync();
            var client = CreateClient();
            // Create a post
            var createRequest = new LinkMeIn.Api.Contracts.Posts.CreatePostRequest
            {
                Title = "Delete Me",
                Content = "To be deleted"
            };
            var createResp = await client.PostAsJsonAsync("/api/posts", createRequest);
            Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
            var createdPost = await createResp.Content.ReadFromJsonAsync<LinkMeIn.Api.Contracts.Posts.PostDto>();
            Assert.NotNull(createdPost);

            // Delete the post
            var deleteResp = await client.DeleteAsync($"/api/posts/{createdPost.Id}");
            Assert.Equal(HttpStatusCode.NoContent, deleteResp.StatusCode);

            // Try to get the deleted post
            var getResp = await client.GetAsync($"/api/posts/{createdPost.Id}");
            Assert.Equal(HttpStatusCode.NotFound, getResp.StatusCode);
        }
        [Fact]
        public async Task UpdatePost_ValidRequest_Returns200AndUpdatedPost()
        {
            await _factory.ResetDatabaseAsync();
            var client = CreateClient();
            // Create a post
            var createRequest = new LinkMeIn.Api.Contracts.Posts.CreatePostRequest
            {
                Title = "Original Title",
                Content = "Original Content"
            };
            var createResp = await client.PostAsJsonAsync("/api/posts", createRequest);
            Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
            var createdPost = await createResp.Content.ReadFromJsonAsync<LinkMeIn.Api.Contracts.Posts.PostDto>();
            Assert.NotNull(createdPost);

            // Update the post
            var updateRequest = new LinkMeIn.Api.Contracts.Posts.UpdatePostRequest
            {
                Title = "Updated Title",
                Content = "Updated Content"
            };
            var updateResp = await client.PutAsJsonAsync($"/api/posts/{createdPost.Id}", updateRequest);
            Assert.Equal(HttpStatusCode.OK, updateResp.StatusCode);
            var updatedPost = await updateResp.Content.ReadFromJsonAsync<LinkMeIn.Api.Contracts.Posts.PostDto>();
            Assert.NotNull(updatedPost);
            Assert.Equal(createdPost.Id, updatedPost.Id);
            Assert.Equal(updateRequest.Title, updatedPost.Title);
            Assert.Equal(updateRequest.Content, updatedPost.Content);
            Assert.Equal("Draft", updatedPost.Status);
        }
        [Fact]
        public async Task GetPostById_NotFound_Returns404()
        {
            await _factory.ResetDatabaseAsync();
            var client = CreateClient();
            var randomId = Guid.NewGuid();
            var resp = await client.GetAsync($"/api/posts/{randomId}");
            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }
        [Fact]
        public async Task GetPostById_Returns200AndCorrectPost()
        {
            await _factory.ResetDatabaseAsync();
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

        public async Task InitializeAsync()
        {
            await _factory.ResetDatabaseAsync();
        }

        public Task DisposeAsync() => Task.CompletedTask;

        private HttpClient CreateClient() => _factory.CreateClient();

        [Fact]
        public async Task GetAllPosts_EmptyDb_Returns200AndEmptyArray()
        {
            await _factory.ResetDatabaseAsync();
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
            await _factory.ResetDatabaseAsync();
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
            await _factory.ResetDatabaseAsync();
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
