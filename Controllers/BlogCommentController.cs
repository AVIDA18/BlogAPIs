using System.Security.Claims;
using System.Text.Json;
using BlogApi.Data;
using BlogApi.DTOs.BlogComment;
using BlogApi.Models;
using BlogApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BlogCommentController : ControllerBase
    {
        private readonly BloggingContext _context;
        private readonly IApiLogger _logger;

        public BlogCommentController(BloggingContext context, IApiLogger logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// This is the api to list all the related comments to a blog.
        /// </summary>
        /// <param name="blogId"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("{blogId}/comments")]
        public async Task<IActionResult> GetCommentsByBlogId(int blogId)
        {
            var comments = await _context.BlogComments
                .Include(c => c.User)
                .Where(c => c.BlogId == blogId)
                .OrderByDescending(c => c.CommentedAt)
                .ToListAsync();

            return Ok(comments);
        }

        /// <summary>
        /// This is the api to add comment on a blog. Only those who has logged in can comment.
        /// </summary>
        /// <param name="blogCommentSaveDto"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("AddBlogComment")]
        public async Task<IActionResult> AddBlogComment([FromBody] BlogCommentSaveDto blogCommentSaveDto)
        {
            var blogComment = new BlogComment
            {
                Comment = blogCommentSaveDto.Comment,
                BlogId = blogCommentSaveDto.BlogId,
                CommentedAt = DateTime.UtcNow,
                UserId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value)
            };

            _context.BlogComments.Add(blogComment);
            await _context.SaveChangesAsync();

            await _logger.LogAsync(
                api: "/BlogCommetn/BlogComment/AddBlogComment",
                payload: JsonSerializer.Serialize(blogComment),
                response: "",
                userId: Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value)
                );

            return Ok(blogComment);
        }


        /// <summary>
        /// This is the api to edit the comment in blog only the admin and person who commented can edit the comment.
        /// </summary>
        /// <param name="blogCommentEditDto"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("EditBlogComment")]
        public async Task<IActionResult> EditBlogComment([FromBody] BlogCommentEditDto blogCommentEditDto)
        {
            var blogComment = await _context.BlogComments.FindAsync(blogCommentEditDto.BlogCommentId);
            if (blogComment == null)
                return NotFound();

            // check if current user is the commenter or Admin
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value);
            string? role = Convert.ToString(User.Claims.FirstOrDefault(c => c.Type == "role")?.Value);
            if (blogComment.UserId == userId || role == "Admin")
            {
                blogComment.Comment = blogCommentEditDto.Comment;
                blogComment.CommentedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                await _logger.LogAsync(
                    api: "/BlogCommetn/BlogComment/EditBlogComment",
                    payload: JsonSerializer.Serialize(blogComment),
                    response: "",
                    userId: Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value)
                    );

                return Ok(blogComment);
            }
            else
            {
                return Forbid();
            }

        }

        /// <summary>
        /// This is the api to delete the comment added only the admin and the person commented can remove it.
        /// </summary>
        /// <param name="commentId"></param>
        /// <returns></returns>
        [Authorize]
        [HttpDelete("DeleteBlogComment/{commentId}")]
        public async Task<IActionResult> DeleteBlogComment(int commentId)
        {
            var blogComment = await _context.BlogComments.FindAsync(commentId);

            if (blogComment == null)
                return NotFound();

            //check if current user is the commenter or admin
            var userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value);
            string? role = Convert.ToString(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value);
            Console.WriteLine($"This is role:{role}");

            if (blogComment.UserId == userId || role == "Admin")
            {
                _context.BlogComments.Remove(blogComment);
                await _context.SaveChangesAsync();

                await _logger.LogAsync(
                    api: $"/BlogCommetn/BlogComment/DeleteBlogComment/{commentId}",
                    payload: JsonSerializer.Serialize(blogComment),
                    response: "",
                    userId: Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value)
                    );                

                return Ok();
            }
            else
            {
                return Forbid();
            }
        }
    }
}