// File: TaskFlow.Core/Models/Category.cs
using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Core.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        // Each category belongs to a user
        public string AppUserId { get; set; } = string.Empty;
        public virtual AppUser? AppUser { get; set; }

        // A category can have many tasks
        public virtual ICollection<TaskItem> TaskItems { get; set; } = new List<TaskItem>();
    }
}