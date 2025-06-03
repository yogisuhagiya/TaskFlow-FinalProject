using System; // For DateTime

namespace TaskFlow.Library.Models
{
    public enum PriorityLevel // You can put enums in their own file or here
    {
        Low,
        Medium,
        High
    }

    public class Task
    {
        public int TaskId { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreationDate { get; set; } = DateTime.UtcNow;
        public DateTime? DueDate { get; set; }
        public PriorityLevel? PriorityLevel { get; set; }
        public string Status { get; set; } = "Pending";
    }
}
