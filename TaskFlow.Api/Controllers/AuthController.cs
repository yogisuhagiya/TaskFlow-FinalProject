// // File: TaskFlow.Api/Controllers/AuthController.cs
// using Microsoft.AspNetCore.Identity;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.IdentityModel.Tokens;
// using System.IdentityModel.Tokens.Jwt;
// using System.Security.Claims;
// using System.Text;
// using TaskFlow.Core.Models;

// public record UserRegisterDto(string Username, string Email, string Password);
// public record UserLoginDto(string Username, string Password);
// public record LoginResponse(string Token);

// [Route("api/[controller]")]
// [ApiController]
// public class AuthController : ControllerBase
// {
//     private readonly UserManager<AppUser> _userManager;
//     private readonly IConfiguration _configuration;
//     public AuthController(UserManager<AppUser> userManager, IConfiguration configuration) {
//         _userManager = userManager; _configuration = configuration;
//     }

//     [HttpPost("register")]
//     public async Task<IActionResult> Register(UserRegisterDto request) {
//         var user = new AppUser { UserName = request.Username, Email = request.Email };
//         var result = await _userManager.CreateAsync(user, request.Password);
//         if (!result.Succeeded) return BadRequest(result.Errors);
//         return Ok("User registered successfully!");
//     }

//     [HttpPost("login")]
//     public async Task<ActionResult<LoginResponse>> Login(UserLoginDto request) {
//         var user = await _userManager.FindByNameAsync(request.Username);
//         if (user != null && await _userManager.CheckPasswordAsync(user, request.Password)) {
//             var claims = new[] { new Claim(ClaimTypes.NameIdentifier, user.Id), new Claim(ClaimTypes.Name, user.UserName!) };
//             var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["AppSettings:Token"]!));
//             var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
//             var token = new JwtSecurityToken(claims: claims, expires: DateTime.Now.AddDays(1), signingCredentials: creds);
//             return Ok(new LoginResponse(new JwtSecurityTokenHandler().WriteToken(token)));
//         }
//         return Unauthorized("Invalid credentials.");
//     }
// }

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskFlow.Core.Models;

public record UserRegisterDto(string Username, string Email, string Password);
public record UserLoginDto(string Username, string Password);
public record LoginResponse(string Token);

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IConfiguration _configuration;

    public AuthController(UserManager<AppUser> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegisterDto request)
    {
        var user = new AppUser { UserName = request.Username, Email = request.Email };
        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded) return BadRequest(result.Errors);
        return Ok("User registered successfully!");
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(UserLoginDto request)
    {
        var user = await _userManager.FindByNameAsync(request.Username);
        if (user != null && await _userManager.CheckPasswordAsync(user, request.Password))
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName!)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["AppSettings:Token"]!));
            
            // THIS IS THE FIX: Using a different, more flexible algorithm
       
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            
            return Ok(new LoginResponse(tokenHandler.WriteToken(token)));
        }
        return Unauthorized("Invalid credentials.");
    }
}