// File: TaskFlow.Data/ApplicationDbContext.cs
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Core.Models;

namespace TaskFlow.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<TaskItem> TaskItems { get; set; }
        public DbSet<Category> Categories { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // Keep this line

            // A task has one category, a category has many tasks
            builder.Entity<TaskItem>()
                .HasOne(t => t.Category)
                .WithMany(c => c.TaskItems)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.NoAction); // This is the corrected line// If a category is deleted, set the task's category to null
        }
    }
}