namespace BlogApi.Models
{
    public class ToDo
    {
        public int Id { get; set; }
        public string Task { get; set; } = string.Empty;
        public DateTime TaskDateTime { get; set; }
        public DateTime TaskAssignedForDateTime { get; set; }
        public bool IsCompleted { get; set; }
        public int EntryByUserId { get; set; }
        public User? User{ get; set; }
    }
}