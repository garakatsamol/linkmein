using LinkMeIn.Api.Contracts.Posts;
using LinkMeIn.Api.Data;
using LinkMeIn.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LinkMeIn.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly LinkMeInDbContext _db;
        private const string DefaultOwnerId = "default-owner";

        public PostsController(LinkMeInDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PostDto>>> GetAll()
        {
            var posts = await _db.Posts
                .Where(p => p.OwnerId == DefaultOwnerId)
                .OrderByDescending(p => p.UpdatedAt)
                .Select(p => new PostDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Content = p.Content,
                    Status = p.Status.ToString(),
                    ScheduledFor = p.ScheduledFor,
                    PublishedAt = p.PublishedAt,
                    LinkedInPostId = p.LinkedInPostId,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                })
                .ToListAsync();
            return Ok(posts);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PostDto>> GetById(Guid id)
        {
            var post = await _db.Posts
                .Where(p => p.Id == id && p.OwnerId == DefaultOwnerId)
                .Select(p => new PostDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Content = p.Content,
                    Status = p.Status.ToString(),
                    ScheduledFor = p.ScheduledFor,
                    PublishedAt = p.PublishedAt,
                    LinkedInPostId = p.LinkedInPostId,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                })
                .FirstOrDefaultAsync();
            if (post == null)
                return NotFound();
            return Ok(post);
        }
    }
}
