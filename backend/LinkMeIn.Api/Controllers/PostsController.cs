using LinkMeIn.Api.Contracts.Posts;
using LinkMeIn.Api.Contracts.Publishing;
using LinkMeIn.Api.Data;
using LinkMeIn.Api.Entities;
using LinkMeIn.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LinkMeIn.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly LinkMeInDbContext _db;
        private readonly IPostPublishingService _postPublishingService;
        private const string DefaultOwnerId = "default-owner";

        public PostsController(LinkMeInDbContext db, IPostPublishingService postPublishingService)
        {
            _db = db;
            _postPublishingService = postPublishingService;
        }

        [HttpPost("{id}/publish")]
        public async Task<ActionResult<PublishPostResponse>> Publish(Guid id, CancellationToken cancellationToken)
        {
            var result = await _postPublishingService.PublishTextPostAsync(id, DefaultOwnerId, cancellationToken);
            var response = new PublishPostResponse
            {
                Success = result.Success,
                Message = result.Message,
                LinkedInPostId = result.LinkedInPostId
            };

            if (result.Success)
            {
                return Ok(response);
            }

            return StatusCode(result.StatusCode, response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var post = await _db.Posts.FirstOrDefaultAsync(p => p.Id == id && p.OwnerId == DefaultOwnerId);
            if (post == null)
                return NotFound();
            _db.Posts.Remove(post);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<PostDto>> Update(Guid id, [FromBody] UpdatePostRequest req)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var post = await _db.Posts.FirstOrDefaultAsync(p => p.Id == id && p.OwnerId == DefaultOwnerId);
            if (post == null)
                return NotFound();

            post.Title = req.Title;
            post.Content = req.Content;
            post.ScheduledFor = req.ScheduledFor;
            post.UpdatedAt = DateTimeOffset.UtcNow;
            post.Status = req.ScheduledFor.HasValue && req.ScheduledFor.Value > post.UpdatedAt
                ? Entities.PostStatus.Scheduled
                : Entities.PostStatus.Draft;

            await _db.SaveChangesAsync();

            var dto = new PostDto
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                Status = post.Status.ToString(),
                ScheduledFor = post.ScheduledFor,
                PublishedAt = post.PublishedAt,
                LinkedInPostId = post.LinkedInPostId,
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt
            };

            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult<PostDto>> Create([FromBody] CreatePostRequest req)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var now = DateTimeOffset.UtcNow;
            var status = req.ScheduledFor.HasValue && req.ScheduledFor.Value > now
                ? Entities.PostStatus.Scheduled
                : Entities.PostStatus.Draft;

            var post = new Entities.Post
            {
                Id = Guid.NewGuid(),
                Title = req.Title,
                Content = req.Content,
                Status = status,
                ScheduledFor = req.ScheduledFor,
                OwnerId = DefaultOwnerId,
                CreatedAt = now,
                UpdatedAt = now
            };
            _db.Posts.Add(post);
            await _db.SaveChangesAsync();

            var dto = new PostDto
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                Status = post.Status.ToString(),
                ScheduledFor = post.ScheduledFor,
                PublishedAt = post.PublishedAt,
                LinkedInPostId = post.LinkedInPostId,
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt
            };

            return CreatedAtAction(nameof(GetById), new { id = post.Id }, dto);
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
