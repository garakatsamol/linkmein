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
        [Fact]
        public async Task DeleteMedia_MediaBelongsToDifferentPost_Returns404()
        {
            await _factory.ResetDatabaseAsync();
            var client = CreateClient();

            // Create post A
            var createRequestA = new LinkMeIn.Api.Contracts.Posts.CreatePostRequest
            {
                Title = "Post A",
                Content = "Content A"
            };
            var createRespA = await client.PostAsJsonAsync("/api/posts", createRequestA);
            Assert.Equal(HttpStatusCode.Created, createRespA.StatusCode);
            var postA = await createRespA.Content.ReadFromJsonAsync<LinkMeIn.Api.Contracts.Posts.PostDto>();
            Assert.NotNull(postA);

            // Create post B
            var createRequestB = new LinkMeIn.Api.Contracts.Posts.CreatePostRequest
            {
                Title = "Post B",
                Content = "Content B"
            };
            var createRespB = await client.PostAsJsonAsync("/api/posts", createRequestB);
            Assert.Equal(HttpStatusCode.Created, createRespB.StatusCode);
            var postB = await createRespB.Content.ReadFromJsonAsync<LinkMeIn.Api.Contracts.Posts.PostDto>();
            Assert.NotNull(postB);

            // Upload a valid image to post A
            var imageBytes = new byte[] {
                0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, // PNG header
                0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52,
                0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01,
                0x08, 0x06, 0x00, 0x00, 0x00, 0x1F, 0x15, 0xC4,
                0x89, 0x00, 0x00, 0x00, 0x0A, 0x49, 0x44, 0x41,
                0x54, 0x78, 0x9C, 0x63, 0x60, 0x00, 0x00, 0x00,
                0x02, 0x00, 0x01, 0xE2, 0x21, 0xBC, 0x33, 0x00,
                0x00, 0x00, 0x00, 0x49, 0x45, 0x4E, 0x44, 0xAE,
                0x42, 0x60, 0x82
            };
            var imageContent = new ByteArrayContent(imageBytes);
            imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
            var form = new MultipartFormDataContent();
            form.Add(imageContent, "file", "test.png");
            var uploadResp = await client.PostAsync($"/api/posts/{postA.Id}/media", form);
            Assert.Equal(HttpStatusCode.Created, uploadResp.StatusCode);
            var mediaDto = await uploadResp.Content.ReadFromJsonAsync<LinkMeIn.Api.Contracts.Media.PostMediaDto>();
            Assert.NotNull(mediaDto);

            // Attempt to delete media from post B (should return 404)
            var deleteResp = await client.DeleteAsync($"/api/posts/{postB.Id}/media/{mediaDto.Id}");
            Assert.Equal(HttpStatusCode.NotFound, deleteResp.StatusCode);
        }
        [Fact]
        public async Task DeleteMedia_NonExistentMedia_Returns404()
        {
            await _factory.ResetDatabaseAsync();
            var client = CreateClient();

            // Create a post
            var createRequest = new LinkMeIn.Api.Contracts.Posts.CreatePostRequest
            {
                Title = "Test Post Delete Nonexistent Media",
                Content = "Test Content"
            };
            var createResp = await client.PostAsJsonAsync("/api/posts", createRequest);
            Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
            var createdPost = await createResp.Content.ReadFromJsonAsync<LinkMeIn.Api.Contracts.Posts.PostDto>();
            Assert.NotNull(createdPost);

            // Generate a random Guid for a non-existent mediaId
            var randomMediaId = Guid.NewGuid();

            // Attempt to delete non-existent media
            var deleteResp = await client.DeleteAsync($"/api/posts/{createdPost.Id}/media/{randomMediaId}");
            Assert.Equal(HttpStatusCode.NotFound, deleteResp.StatusCode);
        }
        [Fact]
        public async Task DeleteMedia_RemovesMediaAndReturns204()
        {
            await _factory.ResetDatabaseAsync();
            var client = CreateClient();

            // Create a post
            var createRequest = new LinkMeIn.Api.Contracts.Posts.CreatePostRequest
            {
                Title = "Test Post Delete Media",
                Content = "Test Content"
            };
            var createResp = await client.PostAsJsonAsync("/api/posts", createRequest);
            Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
            var createdPost = await createResp.Content.ReadFromJsonAsync<LinkMeIn.Api.Contracts.Posts.PostDto>();
            Assert.NotNull(createdPost);

            // Upload a valid image
            var imageBytes = new byte[] {
                0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, // PNG header
                0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52,
                0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01,
                0x08, 0x06, 0x00, 0x00, 0x00, 0x1F, 0x15, 0xC4,
                0x89, 0x00, 0x00, 0x00, 0x0A, 0x49, 0x44, 0x41,
                0x54, 0x78, 0x9C, 0x63, 0x60, 0x00, 0x00, 0x00,
                0x02, 0x00, 0x01, 0xE2, 0x21, 0xBC, 0x33, 0x00,
                0x00, 0x00, 0x00, 0x49, 0x45, 0x4E, 0x44, 0xAE,
                0x42, 0x60, 0x82
            };
            var imageContent = new ByteArrayContent(imageBytes);
            imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
            var form = new MultipartFormDataContent();
            form.Add(imageContent, "file", "test.png");
            var uploadResp = await client.PostAsync($"/api/posts/{createdPost.Id}/media", form);
            Assert.Equal(HttpStatusCode.Created, uploadResp.StatusCode);
            var mediaDto = await uploadResp.Content.ReadFromJsonAsync<LinkMeIn.Api.Contracts.Media.PostMediaDto>();
            Assert.NotNull(mediaDto);

            // Delete the media
            var deleteResp = await client.DeleteAsync($"/api/posts/{createdPost.Id}/media/{mediaDto.Id}");
            Assert.Equal(HttpStatusCode.NoContent, deleteResp.StatusCode);

            // Get media for the post (should be empty)
            var getResp = await client.GetAsync($"/api/posts/{createdPost.Id}/media");
            Assert.Equal(HttpStatusCode.OK, getResp.StatusCode);
            var mediaList = await getResp.Content.ReadFromJsonAsync<List<LinkMeIn.Api.Contracts.Media.PostMediaDto>>();
            Assert.NotNull(mediaList);
            Assert.Empty(mediaList);
        }
        [Fact]
        public async Task UploadMedia_ExceedsMaxImages_Returns400()
        {
            await _factory.ResetDatabaseAsync();
            var client = CreateClient();

            // Create a post
            var createRequest = new LinkMeIn.Api.Contracts.Posts.CreatePostRequest
            {
                Title = "Test Post Max Images",
                Content = "Test Content"
            };
            var createResp = await client.PostAsJsonAsync("/api/posts", createRequest);
            Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
            var createdPost = await createResp.Content.ReadFromJsonAsync<LinkMeIn.Api.Contracts.Posts.PostDto>();
            Assert.NotNull(createdPost);

            // Helper to create a small valid PNG image
            byte[] MakePng(int uniqueByte) => new byte[] {
                0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A,
                0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52,
                0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01,
                0x08, 0x06, 0x00, 0x00, 0x00, (byte)uniqueByte, 0x15, 0xC4,
                0x89, 0x00, 0x00, 0x00, 0x0A, 0x49, 0x44, 0x41,
                0x54, 0x78, 0x9C, 0x63, 0x60, 0x00, 0x00, 0x00,
                0x02, 0x00, 0x01, 0xE2, 0x21, 0xBC, 0x33, 0x00,
                0x00, 0x00, 0x00, 0x49, 0x45, 0x4E, 0x44, 0xAE,
                0x42, 0x60, 0x82
            };

            // Upload 4 valid images (should succeed)
            for (int i = 0; i < 4; i++)
            {
                var imageBytes = MakePng(i);
                var imageContent = new ByteArrayContent(imageBytes);
                imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
                var form = new MultipartFormDataContent();
                form.Add(imageContent, "file", $"img{i}.png");
                var uploadResp = await client.PostAsync($"/api/posts/{createdPost.Id}/media", form);
                Assert.Equal(HttpStatusCode.Created, uploadResp.StatusCode);
            }

            // Upload 5th image (should fail)
            var fifthImageBytes = MakePng(99);
            var fifthImageContent = new ByteArrayContent(fifthImageBytes);
            fifthImageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
            var fifthForm = new MultipartFormDataContent();
            fifthForm.Add(fifthImageContent, "file", "img5.png");
            var fifthUploadResp = await client.PostAsync($"/api/posts/{createdPost.Id}/media", fifthForm);
            Assert.Equal(HttpStatusCode.BadRequest, fifthUploadResp.StatusCode);
        }
        [Fact]
        public async Task UploadMedia_OversizedImage_Returns400()
        {
            await _factory.ResetDatabaseAsync();
            var client = CreateClient();

            // Create a post
            var createRequest = new LinkMeIn.Api.Contracts.Posts.CreatePostRequest
            {
                Title = "Test Post Oversized Image",
                Content = "Test Content"
            };
            var createResp = await client.PostAsJsonAsync("/api/posts", createRequest);
            Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
            var createdPost = await createResp.Content.ReadFromJsonAsync<LinkMeIn.Api.Contracts.Posts.PostDto>();
            Assert.NotNull(createdPost);

            // Create a fake PNG image in memory larger than 2MB (2097152 bytes)
            var oversizedLength = 2097152 + 1;
            var imageBytes = new byte[oversizedLength];
            // PNG header
            imageBytes[0] = 0x89;
            imageBytes[1] = 0x50;
            imageBytes[2] = 0x4E;
            imageBytes[3] = 0x47;
            imageBytes[4] = 0x0D;
            imageBytes[5] = 0x0A;
            imageBytes[6] = 0x1A;
            imageBytes[7] = 0x0A;
            // The rest is just zeroes
            var imageContent = new ByteArrayContent(imageBytes);
            imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");

            var form = new MultipartFormDataContent();
            form.Add(imageContent, "file", "oversized.png");

            // Upload oversized image
            var uploadResp = await client.PostAsync($"/api/posts/{createdPost.Id}/media", form);
            Assert.Equal(HttpStatusCode.BadRequest, uploadResp.StatusCode);
        }
        [Fact]
        public async Task UploadMedia_NonExistentPost_Returns404()
        {
            await _factory.ResetDatabaseAsync();
            var client = CreateClient();

            var randomPostId = Guid.NewGuid();

            // Create a small fake PNG image in memory
            var imageBytes = new byte[] {
                0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, // PNG header
                0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52,
                0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01,
                0x08, 0x06, 0x00, 0x00, 0x00, 0x1F, 0x15, 0xC4,
                0x89, 0x00, 0x00, 0x00, 0x0A, 0x49, 0x44, 0x41,
                0x54, 0x78, 0x9C, 0x63, 0x60, 0x00, 0x00, 0x00,
                0x02, 0x00, 0x01, 0xE2, 0x21, 0xBC, 0x33, 0x00,
                0x00, 0x00, 0x00, 0x49, 0x45, 0x4E, 0x44, 0xAE,
                0x42, 0x60, 0x82
            };
            var imageContent = new ByteArrayContent(imageBytes);
            imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");

            var form = new MultipartFormDataContent();
            form.Add(imageContent, "file", "test.png");

            // Upload image to non-existent post
            var uploadResp = await client.PostAsync($"/api/posts/{randomPostId}/media", form);
            Assert.Equal(HttpStatusCode.NotFound, uploadResp.StatusCode);
        }
        [Fact]
        public async Task UploadMedia_InvalidContentType_Returns400()
        {
            await _factory.ResetDatabaseAsync();
            var client = CreateClient();

            // Create a post
            var createRequest = new LinkMeIn.Api.Contracts.Posts.CreatePostRequest
            {
                Title = "Test Post Invalid ContentType",
                Content = "Test Content"
            };
            var createResp = await client.PostAsJsonAsync("/api/posts", createRequest);
            Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
            var createdPost = await createResp.Content.ReadFromJsonAsync<LinkMeIn.Api.Contracts.Posts.PostDto>();
            Assert.NotNull(createdPost);

            // Create a fake text file in memory
            var fileBytes = System.Text.Encoding.UTF8.GetBytes("This is not an image.");
            var fileContent = new ByteArrayContent(fileBytes);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain");

            var form = new MultipartFormDataContent();
            form.Add(fileContent, "file", "test.txt");

            // Upload file with invalid content type
            var uploadResp = await client.PostAsync($"/api/posts/{createdPost.Id}/media", form);
            Assert.Equal(HttpStatusCode.BadRequest, uploadResp.StatusCode);
        }
        [Fact]
        public async Task GetMedia_NonExistentPost_Returns404()
        {
            await _factory.ResetDatabaseAsync();
            var client = CreateClient();
            var randomPostId = Guid.NewGuid();
            var resp = await client.GetAsync($"/api/posts/{randomPostId}/media");
            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [Fact]
        public async Task UploadMedia_ValidImage_Returns201AndMediaDto()
        {
            await _factory.ResetDatabaseAsync();
            var client = CreateClient();

            // Create a post
            var createRequest = new LinkMeIn.Api.Contracts.Posts.CreatePostRequest
            {
                Title = "Test Post",
                Content = "Test Content"
            };
            var createResp = await client.PostAsJsonAsync("/api/posts", createRequest);
            Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
            var createdPost = await createResp.Content.ReadFromJsonAsync<LinkMeIn.Api.Contracts.Posts.PostDto>();
            Assert.NotNull(createdPost);

            // Create a small fake PNG image in memory
            var imageBytes = new byte[] {
                0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, // PNG header
                0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52,
                0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01,
                0x08, 0x06, 0x00, 0x00, 0x00, 0x1F, 0x15, 0xC4,
                0x89, 0x00, 0x00, 0x00, 0x0A, 0x49, 0x44, 0x41,
                0x54, 0x78, 0x9C, 0x63, 0x60, 0x00, 0x00, 0x00,
                0x02, 0x00, 0x01, 0xE2, 0x21, 0xBC, 0x33, 0x00,
                0x00, 0x00, 0x00, 0x49, 0x45, 0x4E, 0x44, 0xAE,
                0x42, 0x60, 0x82
            };
            var imageContent = new ByteArrayContent(imageBytes);
            imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");

            var form = new MultipartFormDataContent();
            form.Add(imageContent, "file", "test.png");

            // Upload image
            var uploadResp = await client.PostAsync($"/api/posts/{createdPost.Id}/media", form);
            Assert.Equal(HttpStatusCode.Created, uploadResp.StatusCode);

            var mediaDto = await uploadResp.Content.ReadFromJsonAsync<LinkMeIn.Api.Contracts.Media.PostMediaDto>();
            Assert.NotNull(mediaDto);
            Assert.NotEqual(Guid.Empty, mediaDto.Id);
            Assert.Equal(createdPost.Id, mediaDto.PostId);
            Assert.False(string.IsNullOrWhiteSpace(mediaDto.FileName));
            Assert.Equal("image/png", mediaDto.ContentType);
            Assert.True(mediaDto.SizeBytes > 0);
        }

        [Fact]
        public async Task GetMediaContent_ExistingImage_ReturnsImageStream()
        {
            await _factory.ResetDatabaseAsync();
            var client = CreateClient();

            var createRequest = new CreatePostRequest
            {
                Title = "Media Content Test Post",
                Content = "Content with an image"
            };
            var createResp = await client.PostAsJsonAsync("/api/posts", createRequest);
            Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
            var createdPost = await createResp.Content.ReadFromJsonAsync<PostDto>();
            Assert.NotNull(createdPost);

            var imageBytes = new byte[] {
                0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A,
                0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52,
                0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01,
                0x08, 0x06, 0x00, 0x00, 0x00, 0x1F, 0x15, 0xC4,
                0x89, 0x00, 0x00, 0x00, 0x0A, 0x49, 0x44, 0x41,
                0x54, 0x78, 0x9C, 0x63, 0x60, 0x00, 0x00, 0x00,
                0x02, 0x00, 0x01, 0xE2, 0x21, 0xBC, 0x33, 0x00,
                0x00, 0x00, 0x00, 0x49, 0x45, 0x4E, 0x44, 0xAE,
                0x42, 0x60, 0x82
            };
            var imageContent = new ByteArrayContent(imageBytes);
            imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");

            var form = new MultipartFormDataContent();
            form.Add(imageContent, "file", "content-test.png");

            var uploadResp = await client.PostAsync($"/api/posts/{createdPost.Id}/media", form);
            Assert.Equal(HttpStatusCode.Created, uploadResp.StatusCode);
            var mediaDto = await uploadResp.Content.ReadFromJsonAsync<PostMediaDto>();
            Assert.NotNull(mediaDto);

            var contentResp = await client.GetAsync($"/api/posts/{createdPost.Id}/media/{mediaDto.Id}/content");
            Assert.Equal(HttpStatusCode.OK, contentResp.StatusCode);
            Assert.Equal("image/png", contentResp.Content.Headers.ContentType?.MediaType);

            var responseBytes = await contentResp.Content.ReadAsByteArrayAsync();
            Assert.NotEmpty(responseBytes);
        }

        [Fact]
        public async Task GetMediaContent_NonExistentPost_Returns404()
        {
            await _factory.ResetDatabaseAsync();
            var client = CreateClient();

            var postId = Guid.NewGuid();
            var mediaId = Guid.NewGuid();

            var response = await client.GetAsync($"/api/posts/{postId}/media/{mediaId}/content");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetMediaContent_NonExistentMedia_Returns404()
        {
            await _factory.ResetDatabaseAsync();
            var client = CreateClient();

            var createRequest = new CreatePostRequest
            {
                Title = "Media Content Missing Media Test",
                Content = "No matching media"
            };
            var createResp = await client.PostAsJsonAsync("/api/posts", createRequest);
            Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
            var createdPost = await createResp.Content.ReadFromJsonAsync<PostDto>();
            Assert.NotNull(createdPost);

            var mediaId = Guid.NewGuid();

            var response = await client.GetAsync($"/api/posts/{createdPost.Id}/media/{mediaId}/content");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetMediaContent_MediaBelongsToDifferentPost_Returns404()
        {
            await _factory.ResetDatabaseAsync();
            var client = CreateClient();

            var createPostA = new CreatePostRequest
            {
                Title = "Media Owner A",
                Content = "Owns the image"
            };
            var createRespA = await client.PostAsJsonAsync("/api/posts", createPostA);
            Assert.Equal(HttpStatusCode.Created, createRespA.StatusCode);
            var postA = await createRespA.Content.ReadFromJsonAsync<PostDto>();
            Assert.NotNull(postA);

            var createPostB = new CreatePostRequest
            {
                Title = "Media Owner B",
                Content = "Does not own the image"
            };
            var createRespB = await client.PostAsJsonAsync("/api/posts", createPostB);
            Assert.Equal(HttpStatusCode.Created, createRespB.StatusCode);
            var postB = await createRespB.Content.ReadFromJsonAsync<PostDto>();
            Assert.NotNull(postB);

            var imageBytes = new byte[] {
                0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A,
                0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52,
                0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01,
                0x08, 0x06, 0x00, 0x00, 0x00, 0x1F, 0x15, 0xC4,
                0x89, 0x00, 0x00, 0x00, 0x0A, 0x49, 0x44, 0x41,
                0x54, 0x78, 0x9C, 0x63, 0x60, 0x00, 0x00, 0x00,
                0x02, 0x00, 0x01, 0xE2, 0x21, 0xBC, 0x33, 0x00,
                0x00, 0x00, 0x00, 0x49, 0x45, 0x4E, 0x44, 0xAE,
                0x42, 0x60, 0x82
            };
            var imageContent = new ByteArrayContent(imageBytes);
            imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");

            var form = new MultipartFormDataContent();
            form.Add(imageContent, "file", "owned-by-post-a.png");

            var uploadResp = await client.PostAsync($"/api/posts/{postA.Id}/media", form);
            Assert.Equal(HttpStatusCode.Created, uploadResp.StatusCode);
            var mediaDto = await uploadResp.Content.ReadFromJsonAsync<PostMediaDto>();
            Assert.NotNull(mediaDto);

            var response = await client.GetAsync($"/api/posts/{postB.Id}/media/{mediaDto.Id}/content");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

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
