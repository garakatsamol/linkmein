using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinkMeIn.Api.Contracts.Media;
using LinkMeIn.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LinkMeIn.Api.Controllers
{
    [ApiController]
    [Route("api/posts/{postId:guid}/media")]
    public class MediaController : ControllerBase
    {
        private readonly LinkMeInDbContext _db;
        private const string DefaultOwnerId = "default-owner";

        public MediaController(LinkMeInDbContext db)
        {
            _db = db;
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
