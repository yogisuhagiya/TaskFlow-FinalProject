<<<<<<< HEAD
=======

>>>>>>> 504eb3ab2959bb7c5cc20c8be1d7759f45968222
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Library.Dtos;
using TaskFlow.Library.Services;
using System.Threading.Tasks;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
<<<<<<< HEAD
    private readonly IAuthService _authService;

    // Inject the service
    public AuthController(IAuthService authService)
=======
    public class AuthDetails { public string Username { get; set; } = ""; public string Password { get; set; } = ""; }

    [HttpPost("login")] n
    public IActionResult LoginPlaceholder([FromBody] AuthDetails creds)
>>>>>>> 504eb3ab2959bb7c5cc20c8be1d7759f45968222
    {
        _authService = authService;
    }

<<<<<<< HEAD
    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegisterDto request)
=======
    [HttpPost("register")] 
    public IActionResult RegisterPlaceholder([FromBody] AuthDetails creds)
>>>>>>> 504eb3ab2959bb7c5cc20c8be1d7759f45968222
    {
        var user = await _authService.Register(request);
        if (user == null)
        {
            return BadRequest("Username already exists.");
        }
        return Ok(new { Message = "User registered successfully." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(UserLoginDto request)
    {
        var token = await _authService.Login(request);
        if (token == null)
        {
            return Unauthorized("Invalid credentials.");
        }
        return Ok(new { Token = token });
    }
}
