using Microsoft.AspNetCore.Mvc;    
using System.Collections.Generic; 
using System;                      

using AppTask = TaskFlow.Library.Models.Task; 

[ApiController]
[Route("api/tasks")] 
public class TasksController : ControllerBase
{
    [HttpGet] 
    public ActionResult<IEnumerable<AppTask>> GetSampleTasks() // 
    {
        var sampleTasks = new List<AppTask> { 
            new AppTask { TaskId = 1, Title = "Learn API", UserId = 1, Status = "Pending", CreationDate = DateTime.UtcNow }, // <--- USE ALIAS
            new AppTask { TaskId = 2, Title = "Sleep", UserId = 1, Status = "Done", CreationDate = DateTime.UtcNow.AddDays(-1) }      // <--- USE ALIAS
        };
        return Ok(sampleTasks); 
    }
}