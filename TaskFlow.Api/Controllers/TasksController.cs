using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Authorization;

// We no longer need the 'using AppTask = ...' alias.
// Instead, just import the namespace.
using TaskFlow.Library.Models; 

[ApiController]
[Route("api/tasks")]
[Authorize]
public class TasksController : ControllerBase
{
    [HttpGet] 
    // CHANGE THIS: The return type is now a list of TaskItem
    public ActionResult<IEnumerable<TaskItem>> GetMyTasks() 
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        // CHANGE THIS: Use the new name 'TaskItem' here
        var sampleTasks = new List<TaskItem> { 
            new TaskItem { TaskId = 1, Title = "Learn API", UserId = int.Parse(userId!), Status = "Pending" },
            new TaskItem { TaskId = 2, Title = "Sleep", UserId = int.Parse(userId!), Status = "Done" }
        };
        return Ok(sampleTasks); 
    }

    [HttpGet("public")]
    [AllowAnonymous]
    public IActionResult GetPublicTaskInfo()
    {
        return Ok(new { Message = "Anyone can see this public task information!" });
    }
}