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
