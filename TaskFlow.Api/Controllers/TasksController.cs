// namespace TaskFlow.Api.Controllers
// {
//     using Microsoft.AspNetCore.Authorization;
//     using Microsoft.AspNetCore.Mvc;
//     using Microsoft.EntityFrameworkCore;
//     using System.Security.Claims;
//     using TaskFlow.Core.Models;
//     using TaskFlow.Data;

//     [Authorize]
//     [Route("api/[controller]")]
//     [ApiController]
//     public class TasksController : ControllerBase
//     {
//         private readonly ApplicationDbContext _context;
//         public TasksController(ApplicationDbContext context) { _context = context; }

//         private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

//         [HttpGet]
//         public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasks() =>
//             Ok(await _context.TaskItems.Where(t => t.AppUserId == GetUserId()).ToListAsync());

//         [HttpGet("{id}")]
//         public async Task<ActionResult<TaskItem>> GetTask(int id)
//         {
//             var task = await _context.TaskItems.FirstOrDefaultAsync(t => t.Id == id && t.AppUserId == GetUserId());
//             if (task == null) return NotFound();
//             return Ok(task);
//         }

//         [HttpPost]
//         public async Task<ActionResult<TaskItem>> CreateTask(TaskItem taskItem)
//         {
//             taskItem.AppUserId = GetUserId();
//             _context.TaskItems.Add(taskItem);
//             await _context.SaveChangesAsync();
//             return CreatedAtAction(nameof(GetTask), new { id = taskItem.Id }, taskItem);
//         }

//         [HttpPut("{id}")]
//         public async Task<IActionResult> UpdateTask(int id, TaskItem taskItem)
//         {
//             if (id != taskItem.Id) return BadRequest();
//             var taskToUpdate = await _context.TaskItems.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id && t.AppUserId == GetUserId());
//             if (taskToUpdate == null) return NotFound();
//             taskItem.AppUserId = GetUserId();
//             _context.Entry(taskItem).State = EntityState.Modified;
//             await _context.SaveChangesAsync();
//             return NoContent();
//         }

//         [HttpDelete("{id}")]
//         public async Task<IActionResult> DeleteTask(int id)
//         {
//             var task = await _context.TaskItems.FirstOrDefaultAsync(t => t.Id == id && t.AppUserId == GetUserId());
//             if (task == null) return NotFound();
//             _context.TaskItems.Remove(task);
//             await _context.SaveChangesAsync();
//             return NoContent();
//         }

//         [HttpGet("priority/{level}")]
//         public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasksByPriority(PriorityLevel level) =>
//             Ok(await _context.TaskItems.Where(t => t.AppUserId == GetUserId() && t.PriorityLevel == level).ToListAsync());

//         [HttpGet("summary")]
//         public async Task<IActionResult> GetTaskSummary() =>
//             Ok(await _context.TaskItems.Where(t => t.AppUserId == GetUserId()).GroupBy(t => t.Status).Select(g => new { Status = g.Key, Count = g.Count() }).ToListAsync());
//     }
// }
// File: TaskFlow.Api/Controllers/TasksController.cs

namespace TaskFlow.Api.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System.Security.Claims;
    using TaskFlow.Core.Models;
    using TaskFlow.Data;

    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public TasksController(ApplicationDbContext context) { _context = context; }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // Add this simple method for testing

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasks() =>
            Ok(await _context.TaskItems.Where(t => t.AppUserId == GetUserId()).ToListAsync());

        [HttpGet("{id}")]
        public async Task<ActionResult<TaskItem>> GetTask(int id)
        {
            var task = await _context.TaskItems.FirstOrDefaultAsync(t => t.Id == id && t.AppUserId == GetUserId());
            if (task == null) return NotFound();
            return Ok(task);
        }

        // Ancillary Goal #3: Filter tasks by category
        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasksByCategory(int categoryId)
        {
            var userId = GetUserId();
            // First, ensure the user owns the category they are asking for
            var userOwnsCategory = await _context.Categories.AnyAsync(c => c.Id == categoryId && c.AppUserId == userId);
            if (!userOwnsCategory)
            {
                return Forbid(); // Or NotFound, Forbid is more accurate
            }

            var tasks = await _context.TaskItems
                .Where(t => t.AppUserId == userId && t.CategoryId == categoryId)
                .ToListAsync();

            return Ok(tasks);
        }

        [HttpPost]
        public async Task<ActionResult<TaskItem>> CreateTask(TaskItem taskItem)
        {
            taskItem.AppUserId = GetUserId();
            _context.TaskItems.Add(taskItem);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTask), new { id = taskItem.Id }, taskItem);
        }

        // THIS IS THE CORRECTED METHOD THAT FIXES THE FAILING TEST
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, TaskItem taskItem)
        {
            if (id != taskItem.Id)
            {
                return BadRequest();
            }

            // 1. Fetch the existing entity from the database.
            var entityToUpdate = await _context.TaskItems
                .FirstOrDefaultAsync(t => t.Id == id && t.AppUserId == GetUserId());

            // 2. Check if it exists.
            if (entityToUpdate == null)
            {
                return NotFound();
            }

            // 3. Apply the new values to the entity you fetched.
            entityToUpdate.Title = taskItem.Title;
            entityToUpdate.Description = taskItem.Description;
            entityToUpdate.DueDate = taskItem.DueDate;
            entityToUpdate.PriorityLevel = taskItem.PriorityLevel;
            entityToUpdate.Status = taskItem.Status;

            // 4. Save the changes.
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _context.TaskItems.FirstOrDefaultAsync(t => t.Id == id && t.AppUserId == GetUserId());
            if (task == null) return NotFound();
            _context.TaskItems.Remove(task);
            await _context.SaveChangesAsync();
            return NoContent();
        }


        [HttpGet("priority/{level}")]
        public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasksByPriority(PriorityLevel level) =>
            Ok(await _context.TaskItems.Where(t => t.AppUserId == GetUserId() && t.PriorityLevel == level).ToListAsync());

        [HttpGet("summary")]
        public async Task<IActionResult> GetTaskSummary() =>
            Ok(await _context.TaskItems.Where(t => t.AppUserId == GetUserId()).GroupBy(t => t.Status).Select(g => new { Status = g.Key, Count = g.Count() }).ToListAsync());
    }
}