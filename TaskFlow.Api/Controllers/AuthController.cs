using Microsoft.AspNetCore.Mvc;
using TaskFlow.Library.Dtos;
using TaskFlow.Library.Services;
using System.Threading.Tasks;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    // Inject the service
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegisterDto request)
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