using System.ComponentModel.DataAnnotations;

namespace BlogApi.Models
{
    public class BlogComment
    {
        public int Id { get; set; }
        [StringLength(500)]
        public string Comment { get; set; } = string.Empty;
        public DateTime CommentedAt { get; set; }

        //Commented On
        public int BlogId{ get; set; }
        public Blog? Blog{ get; set; }

        //Commented By
        public int UserId { get; set; }
        public User? User { get; set; }
    }
}