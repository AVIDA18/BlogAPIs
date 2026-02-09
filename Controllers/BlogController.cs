using System.Text.Json;
using BlogApi.Data;
using BlogApi.DTOs.Blog;
using BlogApi.Models;
using BlogApi.Services;
using BlogApi.Helper;
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
        private readonly IFileService _fileService;
        private readonly ImageHelper _imageHelper;


        public BlogController(BloggingContext context, IApiLogger logger, IFileService fileService, ImageHelper imageHelper)
        {
            _context = context;
            _logger = logger;
            _fileService = fileService;
            _imageHelper = imageHelper;
        }

        /// <summary>
        /// This is the api that gets all the blogs anyone can use this api to list the blogs.With filters
        /// like select auther wise or categorywise blogs.
        /// </summary>
        /// <returns></returns>
        
        // [HttpGet("getBlogs")]
        // public async Task<IActionResult> GetBlogs()
        // {
        //     var blogs = await _context.Blogs
        //         .Include(b => b.Author)
        //         .OrderByDescending(b => b.CreatedAt)
        //         .ToListAsync();

        //     return Ok(blogs);
        // }

        [HttpGet("getBlogs")]
        public async Task<IActionResult> GetBlogs(
            int page = 1,
            int pageSize = 10,
            int? categoryId = null,
            int? authorId = null)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 10 : pageSize;
            pageSize = Math.Min(pageSize, 50);

            IQueryable<Blog> query = _context.Blogs
                .Include(b => b.Author)
                .Include(b => b.BlogCategory)
                .Include(b => b.Images);

            // Filter by category (if selected)
            if (categoryId.HasValue)
            {
                query = query.Where(b => b.BlogCategoryId == categoryId);
            }

            // Filter by author (if selected)
            if (authorId.HasValue)
            {
                query = query.Where(b => b.AuthorId == authorId);
            }

            var totalCount = await query.CountAsync();

            var blogs = await query
                .OrderByDescending(b => b.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                data = blogs,
                page,
                pageSize,
                totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        }

        [HttpGet("getBlogsByTitleSlug/{slug}")]
        public async Task<IActionResult> GetBlogBySlug(string slug)
        {
            var blog = await _context.Blogs
                .Include(b => b.Author)
                .Include(b => b.BlogCategory)
                .Include(b => b.Images)
                .FirstOrDefaultAsync(b => b.Slug == slug);

            if (blog == null)
                return NotFound();

            return Ok(blog);
        }


        /// <summary>
        /// This is the api to post the blogs only the users with role ="Admin" can post.
        /// </summary>
        /// <param name="blogDto"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpPost("postBlogs")]
        public async Task<IActionResult> CreateBlog([FromForm] BlogSaveDto blogDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var blog = new Blog
                {
                    Title = blogDto.Title,
                    Content = blogDto.Content,
                    Slug = SlugHelper.GenerateSlug(blogDto.Title),
                    CreatedAt = DateTime.UtcNow,
                    AuthorId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value),
                    BlogCategoryId = blogDto.BlogCategoryId
                };

                _context.Blogs.Add(blog);
                await _context.SaveChangesAsync();

                var uploadedImageUrls = blogDto.Files != null 
                    ? await _imageHelper.UploadImagesAsync(blogDto.Files)
                    : new List<string>();

                if (uploadedImageUrls.Any())
                {
                    var images = uploadedImageUrls.Select(url => new BlogApi.Models.BlogImages
                    {
                        BlogId = blog.Id,
                        ImageUrl = url,
                        CreatedAt = DateTime.UtcNow
                    }).ToList();

                    _context.Set<BlogApi.Models.BlogImages>().AddRange(images);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                try
                {
                    await _logger.LogAsync(
                        api: "/Api/Blogs/PostBlogs",
                        payload: JsonSerializer.Serialize(new
                        {
                            blog.Id,
                            blog.Title,
                            blog.BlogCategoryId
                        }),
                        response: "",
                        userId: int.Parse(User.FindFirst("id")!.Value)
                    );
                }
                catch
                {
                    // Logging failure shouldn't break the API
                }

                return Ok(new
                {
                    blog.Id,
                    blog.Title,
                    blog.Content,
                    blog.BlogCategoryId,
                    ImageUrls = uploadedImageUrls
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// This is the api to edit the blogs only admin can edit the blogs.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="blogDto"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpPut("EditBlogs/{id}")]
        public async Task<IActionResult> UpdateBlog(int id, [FromForm] BlogSaveDto blogDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var blog = await _context.Blogs.Include(b => b.Images)
                                            .FirstOrDefaultAsync(b => b.Id == id);
                if (blog == null)
                    return NotFound();

                int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value);

                if (blog.AuthorId != userId)
                    return Forbid();

                blog.Title = blogDto.Title;
                blog.Slug = SlugHelper.GenerateSlug(blogDto.Title);
                blog.Content = blogDto.Content;

                var uploadedImageUrls = blogDto.Files != null 
                    ? await _imageHelper.UploadImagesAsync(blogDto.Files)
                    : new List<string>();

                if (uploadedImageUrls.Any())
                {
                    // Keep track of old images to delete
                    var oldImageUrls = blog.Images.Select(i => i.ImageUrl).ToList();

                    // Remove old images from DB
                    _context.Set<BlogApi.Models.BlogImages>().RemoveRange(blog.Images);

                    // Add new images to DB
                    var newImages = uploadedImageUrls.Select(url => new BlogApi.Models.BlogImages
                    {
                        BlogId = blog.Id,
                        ImageUrl = url,
                        CreatedAt = DateTime.UtcNow
                    }).ToList();
                    _context.Set<BlogApi.Models.BlogImages>().AddRange(newImages);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // Delete old image files (after commit)
                    await _fileService.DeleteImagesAsync(oldImageUrls);
                }
                else
                {
                    // No new files, just update blog content
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }

                try
                {
                    await _logger.LogAsync(
                        api: $"/Api/Blogs/EditBlogs/{id}",
                        payload: JsonSerializer.Serialize(new
                        {
                            blog.Id,
                            blog.Title,
                            blog.BlogCategoryId
                        }),
                        response: "",
                        userId: userId
                    );
                }
                catch
                {
                }

                return Ok(new
                {
                    blog.Id,
                    blog.Title,
                    blog.Content,
                    blog.BlogCategoryId,
                    ImageUrls = uploadedImageUrls
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { error = ex.Message });
            }
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
            var blog = await _context.Blogs.Include(b => b.Images).FirstOrDefaultAsync(b => b.Id == id);
            if (blog == null)
                return NotFound();

            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value);

            if (blog.AuthorId != userId)
                return Forbid();

            var imageUrls = blog.Images.Select(i => i.ImageUrl).ToList();

            _context.Blogs.Remove(blog);
            await _context.SaveChangesAsync();

            await _fileService.DeleteImagesAsync(imageUrls);

            await _logger.LogAsync(
                api: $"/Api/Blogs/DeleteBlogs/{id}",
                payload: JsonSerializer.Serialize(blog),
                response: "",
                userId: Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value)
                );

            return Ok();
        }
    }
}