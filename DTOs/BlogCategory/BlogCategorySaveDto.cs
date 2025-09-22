using System.ComponentModel.DataAnnotations;

namespace BlogApi.DTOs.BlogCategory
{
    public class BlogCategorySaveDto
    {
        [Required]
        public string CategoryName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}