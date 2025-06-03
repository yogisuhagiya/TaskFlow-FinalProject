// In TaskFlow.Api/Controllers/AuthController.cs
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/auth")] // Web address will be /api/auth
public class AuthController : ControllerBase
{
    // Simple class to hold login/register info
    public class AuthDetails { public string Username { get; set; } = ""; public string Password { get; set; } = ""; }

    [HttpPost("login")] // For when someone POSTS to /api/auth/login
    public IActionResult LoginPlaceholder([FromBody] AuthDetails creds)
    {
        return Ok(new { Message = "Login will go here!" });
    }

    [HttpPost("register")] // For /api/auth/register
    public IActionResult RegisterPlaceholder([FromBody] AuthDetails creds)
    {
        return Ok(new { Message = "Registration will go here!" });
    }
}