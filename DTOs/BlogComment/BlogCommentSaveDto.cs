using System.ComponentModel.DataAnnotations;

namespace BlogApi.DTOs.BlogComment
{
    public class BlogCommentSaveDto
    {
        [Required]
        public string Comment { get; set; } = string.Empty;
        [Required]
        public int BlogId{ get; set; }
    }
}