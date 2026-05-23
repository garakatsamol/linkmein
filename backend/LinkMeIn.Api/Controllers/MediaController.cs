using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinkMeIn.Api.Contracts.Media;
using LinkMeIn.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LinkMeIn.Api.Options;
using LinkMeIn.Api.Services;
using Microsoft.Extensions.Options;

namespace LinkMeIn.Api.Controllers
{
    [ApiController]
    [Route("api/posts/{postId:guid}/media")]
    public class MediaController : ControllerBase
    {
        [HttpDelete("{mediaId:guid}")]
        public async Task<IActionResult> DeleteMedia(Guid postId, Guid mediaId)
        {
            // Find post
            var post = await _db.Posts.FirstOrDefaultAsync(p => p.Id == postId && p.OwnerId == DefaultOwnerId);
            if (post == null)
                return NotFound();

            // Find media
            var media = await _db.PostMedia.FirstOrDefaultAsync(m => m.Id == mediaId && m.PostId == postId);
            if (media == null)
                return NotFound();

            // Delete physical file (ignore if file does not exist)
            try
            {
                await _mediaStorage.DeleteFileAsync(media.StoragePath);
            }
            catch (Exception)
            {
                // Ignore file not found or deletion errors unless the storage service throws for non-existence
            }

            // Remove DB record
            _db.PostMedia.Remove(media);
            await _db.SaveChangesAsync();

            return NoContent();
        }
        private readonly LinkMeInDbContext _db;
        private readonly IMediaStorageService _mediaStorage;
        private readonly MediaStorageOptions _mediaOptions;
        private const string DefaultOwnerId = "default-owner";

        public MediaController(LinkMeInDbContext db, IMediaStorageService mediaStorage, Microsoft.Extensions.Options.IOptions<MediaStorageOptions> mediaOptions)
        {
            _db = db;
            _mediaStorage = mediaStorage;
            _mediaOptions = mediaOptions.Value;
        }
        [HttpPost]
        public async Task<ActionResult<PostMediaDto>> UploadMedia(Guid postId, [FromForm] Microsoft.AspNetCore.Http.IFormFile file)
        {
            // Find post
            var post = await _db.Posts.FirstOrDefaultAsync(p => p.Id == postId && p.OwnerId == DefaultOwnerId);
            if (post == null)
                return NotFound();

            // Validate file present
            if (file == null || file.Length == 0)
                return BadRequest("File is required.");

            // Validate content type
            if (!_mediaOptions.AllowedContentTypes.Contains(file.ContentType))
                return BadRequest($"Content type '{file.ContentType}' is not allowed.");

            // Validate file size
            if (file.Length > _mediaOptions.MaxFileSizeBytes)
                return BadRequest($"File size exceeds maximum of {_mediaOptions.MaxFileSizeBytes} bytes.");

            // Validate max images per post
            var mediaCount = await _db.PostMedia.CountAsync(m => m.PostId == postId);
            if (mediaCount >= _mediaOptions.MaxImagesPerPost)
                return BadRequest($"Maximum of {_mediaOptions.MaxImagesPerPost} images per post exceeded.");

            // Save file
            string storagePath;
            using (var stream = file.OpenReadStream())
            {
                storagePath = await _mediaStorage.SaveFileAsync(postId.ToString(), file.FileName, stream, file.ContentType);
            }

            // Create PostMedia record
            var postMedia = new Entities.PostMedia
            {
                Id = Guid.NewGuid(),
                PostId = postId,
                OwnerId = DefaultOwnerId,
                FileName = file.FileName,
                ContentType = file.ContentType,
                SizeBytes = file.Length,
                StoragePath = storagePath,
                CreatedAt = DateTimeOffset.UtcNow
            };
            _db.PostMedia.Add(postMedia);
            await _db.SaveChangesAsync();

            var dto = new PostMediaDto
            {
                Id = postMedia.Id,
                PostId = postMedia.PostId,
                FileName = postMedia.FileName,
                ContentType = postMedia.ContentType,
                SizeBytes = postMedia.SizeBytes,
                CreatedAt = postMedia.CreatedAt,
                LinkedInAssetUrn = postMedia.LinkedInAssetUrn
            };
            return CreatedAtAction(nameof(GetMedia), new { postId }, dto);
        }

        [HttpGet]
        public async Task<ActionResult<List<PostMediaDto>>> GetMedia(Guid postId)
        {
            var post = await _db.Posts
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == postId && p.OwnerId == DefaultOwnerId);
            if (post == null)
                return NotFound();

            var media = await _db.PostMedia
                .AsNoTracking()
                .Where(m => m.PostId == postId)
                .Select(m => new PostMediaDto
                {
                    Id = m.Id,
                    PostId = m.PostId,
                    FileName = m.FileName,
                    ContentType = m.ContentType,
                    SizeBytes = m.SizeBytes,
                    CreatedAt = m.CreatedAt,
                    LinkedInAssetUrn = m.LinkedInAssetUrn
                })
                .ToListAsync();

            return Ok(media);
        }
    }
}
