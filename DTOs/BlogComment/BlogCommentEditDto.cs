using System.ComponentModel.DataAnnotations;

namespace BlogApi.DTOs.BlogComment
{
    public class BlogCommentEditDto
    {
        [Required]
        public int BlogCommentId { get; set; }
        [Required]
        public string Comment { get; set; } = string.Empty;
    }
}