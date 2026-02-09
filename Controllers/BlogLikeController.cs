using System.Text.Json;
using BlogApi.Data;
using BlogApi.DTOs.BlogLike;
using BlogApi.Models;
using BlogApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BlogLikeController : ControllerBase
    {
        private readonly BloggingContext _context;
        private readonly IApiLogger _logger;

        public BlogLikeController(BloggingContext context, IApiLogger logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// This is the api to list all the related likes to a blog.
        /// </summary>
        /// <param name="blogId"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("{blogId}/Likes")]
        public async Task<IActionResult> GetLikesByBlogId(int blogId)
        {
            var comments = await _context.BlogLikes
                .Include(c => c.User)
                .Where(c => c.BlogId == blogId)
                .OrderByDescending(c => c.LikedAt)
                .ToListAsync();

            return Ok(comments);
        }

        /// <summary>
        /// Api to add like if the blog is liked.
        /// 
        /// </summary>
        /// <param name="likeSaveDto"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("AddBlogLike")]
        public async Task<IActionResult> AddBlogLike([FromBody] BlogLikeSaveDto likeSaveDto)
        {
            var blogLike = new BlogLike
            {
                Like = likeSaveDto.Like,
                BlogId = likeSaveDto.BlogId,
                LikedAt = DateTime.UtcNow,
                UserId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value)
            };

            _context.BlogLikes.Add(blogLike);
            await _context.SaveChangesAsync();

            await _logger.LogAsync(
                api: "/Api/BlogLike/AddBlogLike",
                payload: JsonSerializer.Serialize(blogLike),
                response: "",
                userId: Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value)
                );

            return Ok(blogLike);
        }


        /// <summary>
        /// Api to remove like. Only the user who liked the blog can remove it. 
        /// </summary>
        /// <param name="blogId"></param>
        /// <returns></returns>
        [Authorize]
        [HttpDelete("RemoveBlogLike/{blogId}")]
        public async Task<IActionResult> DeleteBlogLike(int blogId)
        {
            var userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value);

            var blogLike = await _context.BlogLikes
                .FirstOrDefaultAsync(bl => bl.BlogId == blogId && bl.UserId == userId);


            if (blogLike == null)
                return NotFound();

            _context.BlogLikes.Remove(blogLike);
            await _context.SaveChangesAsync();

            await _logger.LogAsync(
                api: $"/Api/BlogLike/RemoveBlogLike/{blogId}",
                payload: JsonSerializer.Serialize(blogLike),
                response: "",
                userId: Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value)
                );

            return Ok();
        }
    }
}