using System.ComponentModel.DataAnnotations;

namespace BlogApi.DTOs.Blog
{
    public class BlogSaveDto
    {
        [Required]
        public int BlogCategoryId { get; set; }
        [Required]
        public string Title { get; set; } = string.Empty;
        [Required]
        public string Content { get; set; } = string.Empty;
    }
}