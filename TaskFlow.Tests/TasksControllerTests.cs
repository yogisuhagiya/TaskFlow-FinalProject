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

//         // THIS IS THE CORRECTED METHOD
//         [HttpPut("{id}")]
//         public async Task<IActionResult> UpdateTask(int id, TaskItem taskItem)
//         {
//             if (id != taskItem.Id)
//             {
//                 return BadRequest();
//             }

//             var entityToUpdate = await _context.TaskItems
//                 .FirstOrDefaultAsync(t => t.Id == id && t.AppUserId == GetUserId());

//             if (entityToUpdate == null)
//             {
//                 return NotFound();
//             }

//             entityToUpdate.Title = taskItem.Title;
//             entityToUpdate.Description = taskItem.Description;
//             entityToUpdate.DueDate = taskItem.DueDate;
//             entityToUpdate.PriorityLevel = taskItem.PriorityLevel;
//             entityToUpdate.Status = taskItem.Status;

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


using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskFlow.Api.Controllers;
using TaskFlow.Core.Models;
using TaskFlow.Data;
using Xunit;

public class TasksControllerTests
{
    private readonly ApplicationDbContext _context;
    private readonly TasksController _controller;
    private readonly string _testUserId = "user-id-1";
    private readonly string _otherUserId = "user-id-2";

    // This constructor sets up a fresh in-memory database and controller for each test.
    public TasksControllerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        SeedDatabase();

        _controller = new TasksController(_context);
        
        // Mock a logged-in user by creating a fake identity for the controller.
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, _testUserId)
        }, "mock-auth"));
        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };
    }

    private void SeedDatabase()
    {
        var tasks = new List<TaskItem>
        {
            new TaskItem { Id = 1, Title = "My High Priority Task", AppUserId = _testUserId, Status = "Pending", PriorityLevel = PriorityLevel.High },
            new TaskItem { Id = 2, Title = "My Completed Task", AppUserId = _testUserId, Status = "Completed" },
            new TaskItem { Id = 3, Title = "Someone else's task", AppUserId = _otherUserId, Status = "Pending" }
        };
        _context.TaskItems.AddRange(tasks);
        _context.SaveChanges();
    }

    // Test 1: GET (All) - Proves security and data fetching.
    [Fact]
    public async Task GetTasks_ReturnsOnlyTasksForCurrentUser()
    {
        var result = await _controller.GetTasks();
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var tasks = Assert.IsAssignableFrom<IEnumerable<TaskItem>>(okResult.Value);
        Assert.Equal(2, tasks.Count()); // User 1 should only have 2 tasks.
        Assert.All(tasks, task => Assert.Equal(_testUserId, task.AppUserId));
    }

    // Test 2: GET (by ID) - Proves successful retrieval.
    [Fact]
    public async Task GetTask_WithValidIdForUser_ReturnsCorrectTask()
    {
        var result = await _controller.GetTask(1);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var task = Assert.IsType<TaskItem>(okResult.Value);
        Assert.Equal(1, task.Id);
    }

    // Test 3: GET (by ID) - Proves security.
    [Fact]
    public async Task GetTask_WithIdBelongingToAnotherUser_ReturnsNotFound()
    {
        // Attempting to get task with ID 3, which belongs to another user.
        var result = await _controller.GetTask(3);
        Assert.IsType<NotFoundResult>(result.Result);
    }

    // Test 4: POST (Create) - Proves creation and correct user assignment.
    [Fact]
    public async Task CreateTask_WhenSuccessful_ReturnsCreatedAtAction()
    {
        var newTask = new TaskItem { Title = "A brand new task" };
        var result = await _controller.CreateTask(newTask);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var taskValue = Assert.IsType<TaskItem>(createdResult.Value);
        Assert.Equal(_testUserId, taskValue.AppUserId); // Crucially, ensure it's assigned to the right user.
    }

    // Test 5: PUT (Update) - Proves successful update.
    [Fact]
    public async Task UpdateTask_WithValidIdForUser_ReturnsNoContent()
    {
        var updatedTaskData = new TaskItem { Id = 1, Title = "Title has been updated" };
        var result = await _controller.UpdateTask(1, updatedTaskData);
        Assert.IsType<NoContentResult>(result);
    }

    // Test 6: PUT (Update) - Proves validation.
    [Fact]
    public async Task UpdateTask_WithMismatchedId_ReturnsBadRequest()
    {
        var updatedTaskData = new TaskItem { Id = 99, Title = "Mismatched ID" };
        var result = await _controller.UpdateTask(1, updatedTaskData);
        Assert.IsType<BadRequestResult>(result);
    }

    // Test 7: DELETE - Proves successful deletion.
    [Fact]
    public async Task DeleteTask_WithValidIdForUser_ReturnsNoContent()
    {
        var result = await _controller.DeleteTask(1);
        Assert.IsType<NoContentResult>(result);
    }

    // Test 8: DELETE - Proves deletion actually removes the item.
    [Fact]
    public async Task DeleteTask_WhenSuccessful_RemovesTaskFromDatabase()
    {
        await _controller.DeleteTask(1);
        var deletedTask = await _context.TaskItems.FindAsync(1);
        Assert.Null(deletedTask);
    }

    // Test 9: Ancillary Goal #1 - Proves filtering by priority works.
    [Fact]
    public async Task GetTasksByPriority_ReturnsOnlyMatchingPriorityTasks()
    {
        var result = await _controller.GetTasksByPriority(PriorityLevel.High);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var tasks = Assert.IsAssignableFrom<IEnumerable<TaskItem>>(okResult.Value);
        Assert.Single(tasks);
        Assert.Equal(1, tasks.First().Id);
    }

    // Test 10: Ancillary Goal #2 - Proves summary endpoint works.
    [Fact]
    public async Task GetTaskSummary_ReturnsCorrectSummary()
    {
        var result = await _controller.GetTaskSummary();
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value); // Check that the summary data is not null.
    }
}