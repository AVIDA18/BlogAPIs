using System.Text.Json;
using BlogApi.Data;
using BlogApi.DTOs.Blog;
using BlogApi.Models;
using BlogApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BlogController : ControllerBase
    {
        private readonly BloggingContext _context;
        private readonly IApiLogger _logger;

        public BlogController(BloggingContext context, IApiLogger logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// This is the api that gets all the blogs anyone can use this api to list the blogs.
        /// </summary>
        /// <returns></returns>
        [HttpGet("getBlogs")]
        public async Task<IActionResult> GetBlogs()
        {
            var blogs = await _context.Blogs
                .Include(b => b.Author)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return Ok(blogs);
        }


        /// <summary>
        /// This is the api to post the blogs only the users with role ="Admin" can post.
        /// </summary>
        /// <param name="blogDto"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpPost("postBlogs")]
        public async Task<IActionResult> CreateBlog([FromBody] BlogSaveDto blogDto)
        {
            var blog = new Blog
            {
                Title = blogDto.Title,
                Content = blogDto.Content,
                CreatedAt = DateTime.UtcNow,
                AuthorId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value)
            };

            _context.Blogs.Add(blog);
            await _context.SaveChangesAsync();

            await _logger.LogAsync(
                api: "/Api/Blogs/PostBlogs",
                payload: JsonSerializer.Serialize(blog),
                response: "",
                userId: Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value)
                );

            return Ok(blog);
        }

        /// <summary>
        /// This is the api to edit the blogs only admin can edit the blogs.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="blogDto"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpPut("EditBlogs/{id}")]
        public async Task<IActionResult> UpdateBlog(int id, [FromBody] BlogSaveDto blogDto)
        {
            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null)
                return NotFound();

            // check if current user is the author
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value);

            if (blog.AuthorId != userId)
                return Forbid();

            blog.Title = blogDto.Title;
            blog.Content = blogDto.Content;

            await _context.SaveChangesAsync();

            await _logger.LogAsync(
                api: "/Api/Blogs/EditBlogs",
                payload: JsonSerializer.Serialize(blog),
                response: "",
                userId: Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value)
                );

            return Ok(blog);
        }

        /// <summary>
        /// This is the api to delete the blogs. Deleting
        /// blogs delets all the commetns and likes related to it.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpDelete("DeleteBlogs/{id}")]
        public async Task<IActionResult> DeleteBlog(int id)
        {
            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null)
                return NotFound();

            // check if current user is the author
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value);

            if (blog.AuthorId != userId)
                return Forbid();

            _context.Blogs.Remove(blog);
            await _context.SaveChangesAsync();

            await _logger.LogAsync(
                api: "/Api/Blogs/DeleteBlogs",
                payload: JsonSerializer.Serialize(blog),
                response: "",
                userId: Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value)
                );

            return Ok();
        }
    }
}