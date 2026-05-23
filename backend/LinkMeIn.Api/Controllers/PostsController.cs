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
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PostDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Content = p.Content,
                    ScheduledFor = p.ScheduledFor,
                    Status = p.Status.ToString(),
                    CreatedAt = p.CreatedAt.UtcDateTime,
                    UpdatedAt = p.UpdatedAt.UtcDateTime
                })
                .ToListAsync();
            return Ok(posts);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PostDto>> GetById(Guid id)
        {
            var post = await _db.Posts.FirstOrDefaultAsync(p => p.Id == id && p.OwnerId == DefaultOwnerId);
            if (post == null) return NotFound();
            return Ok(new PostDto
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                ScheduledFor = post.ScheduledFor,
                Status = post.Status.ToString(),
                CreatedAt = post.CreatedAt.UtcDateTime,
                UpdatedAt = post.UpdatedAt.UtcDateTime
            });
        }

        [HttpPost]
        public async Task<ActionResult<PostDto>> Create(CreatePostRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Title))
                return BadRequest("Title is required.");
            if (string.IsNullOrWhiteSpace(req.Content))
                return BadRequest("Content is required.");

            var now = DateTimeOffset.UtcNow;
            var status = req.ScheduledFor.HasValue && req.ScheduledFor.Value > now
                ? PostStatus.Scheduled
                : PostStatus.Draft;

            var post = new Post
            {
                Id = Guid.NewGuid(),
                Title = req.Title,
                Content = req.Content,
                ScheduledFor = req.ScheduledFor,
                Status = status,
                OwnerId = DefaultOwnerId,
                CreatedAt = now,
                UpdatedAt = now
            };
            _db.Posts.Add(post);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = post.Id }, new PostDto
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                ScheduledFor = post.ScheduledFor,
                Status = post.Status.ToString(),
                CreatedAt = post.CreatedAt.UtcDateTime,
                UpdatedAt = post.UpdatedAt.UtcDateTime
            });
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<PostDto>> Update(Guid id, UpdatePostRequest req)
        {
            var post = await _db.Posts.FirstOrDefaultAsync(p => p.Id == id && p.OwnerId == DefaultOwnerId);
            if (post == null) return NotFound();
            if (string.IsNullOrWhiteSpace(req.Title))
                return BadRequest("Title is required.");
            if (string.IsNullOrWhiteSpace(req.Content))
                return BadRequest("Content is required.");

            var now = DateTimeOffset.UtcNow;
            post.Title = req.Title;
            post.Content = req.Content;
            post.ScheduledFor = req.ScheduledFor;
            post.Status = req.ScheduledFor.HasValue && req.ScheduledFor.Value > now
                ? PostStatus.Scheduled
                : PostStatus.Draft;
            post.UpdatedAt = now;
            await _db.SaveChangesAsync();
            return Ok(new PostDto
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                ScheduledFor = post.ScheduledFor,
                Status = post.Status.ToString(),
                CreatedAt = post.CreatedAt.UtcDateTime,
                UpdatedAt = post.UpdatedAt.UtcDateTime
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var post = await _db.Posts.FirstOrDefaultAsync(p => p.Id == id && p.OwnerId == DefaultOwnerId);
            if (post == null) return NotFound();
            _db.Posts.Remove(post);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
