// File: TaskFlow.Core/Models/TaskItem.cs
using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Core.Models
{
    public enum PriorityLevel { Low, Medium, High }

    public class TaskItem
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreationDate { get; set; } = DateTime.UtcNow;
        public DateTime? DueDate { get; set; }
        public PriorityLevel? PriorityLevel { get; set; }
        public string Status { get; set; } = "Pending";
        public string AppUserId { get; set; } = string.Empty;
        public virtual AppUser? AppUser { get; set; }
        // Add these two properties to the TaskItem class
        public int? CategoryId { get; set; } // Nullable, as a task might not have a category
        public virtual Category? Category { get; set; }
    }
}