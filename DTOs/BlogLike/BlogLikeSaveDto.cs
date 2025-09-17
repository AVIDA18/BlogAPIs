using System.ComponentModel.DataAnnotations;

namespace BlogApi.DTOs.BlogLike
{
    public class BlogLikeSaveDto
    {
        [Required]
        public bool Like { get; set; }
        [Required]
        public int BlogId{ get; set; } 
    }
}