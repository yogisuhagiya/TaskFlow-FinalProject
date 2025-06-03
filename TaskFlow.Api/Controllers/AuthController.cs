
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    public class AuthDetails { public string Username { get; set; } = ""; public string Password { get; set; } = ""; }

    [HttpPost("login")] n
    public IActionResult LoginPlaceholder([FromBody] AuthDetails creds)
    {
        return Ok(new { Message = "Login will go here!" });
    }

    [HttpPost("register")] 
    public IActionResult RegisterPlaceholder([FromBody] AuthDetails creds)
    {
        return Ok(new { Message = "Registration will go here!" });
    }
}
