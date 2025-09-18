namespace BlogApi.DTOs.ToDo
{
    public class ToDoSaveEditDto
    {
        public string Task { get; set; } = string.Empty;
        public DateTime TaskDateTime { get; set; }
    }
}