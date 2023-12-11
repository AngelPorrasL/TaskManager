namespace TasksPage.Models
{
    public class Tasks
    {
        public string? Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? DueDate { get; set; }
        public int? IsCompleted { get; set; }
        public string? Priority { get; set; }
    }
}
