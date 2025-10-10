using System.Text.Json;
using BlogApi.Data;
using BlogApi.DTOs.BlogCategory;
using BlogApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BlogCategoryController : ControllerBase
    {
        private readonly BloggingContext _context;
        private readonly IApiLogger _logger;

        public BlogCategoryController(BloggingContext context, IApiLogger logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// The api to list the blog cateogories
        /// </summary>
        /// <returns></returns>
        [HttpGet("listBlogCategories")]
        public async Task<IActionResult> GetBlogCategories()
        {
            var categories = await _context.BlogCategories.ToListAsync();
            if (categories == null || !categories.Any())
            {
                return NotFound("No blog categories found.");
            }
            return Ok(categories);
        }

        /// <summary>
        /// Api to add new blog category.
        /// </summary>
        /// <param name="blogSaveDto"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpPost("addBlogCategory")]
        public async Task<IActionResult> AddNewBlogCategory([FromBody] BlogCategorySaveDto saveDto)
        {
            var newCategory = new Models.BlogCategory
            {
                CategoryName = saveDto.CategoryName,
                Description = saveDto.Description
            };

            _context.BlogCategories.Add(newCategory);
            await _context.SaveChangesAsync();

            await _logger.LogAsync(
                api: "/Api/BlogCategory/addBlogCategory",
                payload: JsonSerializer.Serialize(newCategory),
                response: "",
                userId: Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value)
                );

            return Ok(newCategory);
        }

        /// <summary>
        /// Api to edit existing blog category
        /// </summary>
        /// <param name="id"></param>
        /// <param name="blogSaveDto"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpPut("EditBlogCategory/{id}")]
        public async Task<IActionResult> EditBlogCategory(int id, [FromBody] BlogCategorySaveDto saveDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var category = await _context.BlogCategories.FindAsync(id);
            if (category == null)
            {
                return NotFound($"No BlogCategory found with ID {id}");
            }

            category.CategoryName = saveDto.CategoryName;
            category.Description = saveDto.Description;

            await _context.SaveChangesAsync();

            await _logger.LogAsync(
                api: $"/Api/BlogCategory/EditBlogCategory/{id}",
                payload: JsonSerializer.Serialize(saveDto),
                response: "",
                userId: Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value)
            );

            return Ok(category);
        }

        /// <summary>
        /// Delete blog category
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpDelete("DeleteBlogCategory/{id}")]
        public async Task<IActionResult> DeleteBlogCategory(int id)
        {
            var category = await _context.BlogCategories.FindAsync(id);
            if (category == null)
            {
                return NotFound($"Blog category with ID {id} not found.");
            }

            _context.BlogCategories.Remove(category);
            await _context.SaveChangesAsync();

            await _logger.LogAsync(
                api: $"/Api/BlogCategory/DeleteBlogCategory/{id}",
                payload: JsonSerializer.Serialize(category),
                response: "",
                userId: Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value)
                );

            return NoContent();
        }
    }
}