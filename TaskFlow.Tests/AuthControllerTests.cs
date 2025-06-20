// File: TaskFlow.Tests/AuthControllerTests.cs

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Security.Claims; // You might need this for login tests later
using TaskFlow.Api.Controllers;
using TaskFlow.Core.Models;
using Xunit;

public class AuthControllerTests
{
    // This helper method correctly mocks UserManager for testing.
    private Mock<UserManager<AppUser>> GetMockUserManager()
    {
        var store = new Mock<IUserStore<AppUser>>();
        var options = new Mock<IOptions<IdentityOptions>>();
        var identityOptions = new IdentityOptions();
        options.Setup(o => o.Value).Returns(identityOptions);
        var userValidators = new List<IUserValidator<AppUser>>();
        var passValidators = new List<IPasswordValidator<AppUser>>();

        return new Mock<UserManager<AppUser>>(
            store.Object,
            options.Object,
            new Mock<IPasswordHasher<AppUser>>().Object,
            userValidators,
            passValidators,
            new Mock<ILookupNormalizer>().Object,
            new Mock<IdentityErrorDescriber>().Object,
            new Mock<IServiceProvider>().Object,
            new Mock<ILogger<UserManager<AppUser>>>().Object);
    }

    // This helper method correctly mocks IConfiguration for the JWT token.
    private Mock<IConfiguration> GetMockConfiguration()
    {
        var mockConfiguration = new Mock<IConfiguration>();
        var mockAppSection = new Mock<IConfigurationSection>();
        
        // This setup is more robust for ["AppSettings:Token"]
        mockAppSection.Setup(x => x.Value).Returns("a-very-long-and-secure-secret-key-that-is-guaranteed-to-be-long-enough");
        mockConfiguration.Setup(x => x.GetSection("AppSettings:Token")).Returns(mockAppSection.Object);

        return mockConfiguration;
    }

    #region Register Tests
    [Fact]
    public async Task Register_WithValidData_ReturnsOk()
    {
        // Arrange
        var mockUserManager = GetMockUserManager();
        mockUserManager.Setup(x => x.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
                       .ReturnsAsync(IdentityResult.Success); // Mock the success case
        
        var controller = new AuthController(mockUserManager.Object, GetMockConfiguration().Object);
        var userDto = new UserRegisterDto("newUser", "test@test.com", "Password123!");

        // Act
        var result = await controller.Register(userDto);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Register_WhenCreationFails_ReturnsBadRequest()
    {
        // Arrange
        var mockUserManager = GetMockUserManager();
        // This simulates any failure from UserManager (e.g., duplicate username, weak password)
        mockUserManager.Setup(x => x.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
                       .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Creation failed." }));

        var controller = new AuthController(mockUserManager.Object, GetMockConfiguration().Object);
        var userDto = new UserRegisterDto("existingUser", "test@test.com", "Password123!");

        // Act
        var result = await controller.Register(userDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
    #endregion

    #region Login Tests
    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithToken()
    {
        // Arrange
        var mockUserManager = GetMockUserManager();
        var userDto = new UserLoginDto("validUser", "Password123!");
        var appUser = new AppUser { Id = "1", UserName = "validUser" };
        
        mockUserManager.Setup(x => x.FindByNameAsync(userDto.Username)).ReturnsAsync(appUser);
        mockUserManager.Setup(x => x.CheckPasswordAsync(appUser, userDto.Password)).ReturnsAsync(true);
        
        var controller = new AuthController(mockUserManager.Object, GetMockConfiguration().Object);

        // Act
        var result = await controller.Login(userDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<LoginResponse>(okResult.Value);
        Assert.False(string.IsNullOrEmpty(response.Token)); // Ensure a token was actually generated
    }

    [Fact]
    public async Task Login_WithInvalidUsername_ReturnsUnauthorized()
    {
        // Arrange
        var mockUserManager = GetMockUserManager();

        // THE FIX: Use Task.FromResult to handle async methods that can return null.
        mockUserManager.Setup(x => x.FindByNameAsync(It.IsAny<string>()))
                       .Returns(Task.FromResult<AppUser?>(null));

        var controller = new AuthController(mockUserManager.Object, GetMockConfiguration().Object);
        var userDto = new UserLoginDto("invalidUser", "Password123!");

        // Act
        var result = await controller.Login(userDto);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        var mockUserManager = GetMockUserManager();
        var userDto = new UserLoginDto("validUser", "wrongPassword");
        var appUser = new AppUser { Id = "1", UserName = "validUser" };

        mockUserManager.Setup(x => x.FindByNameAsync(userDto.Username)).ReturnsAsync(appUser);
        mockUserManager.Setup(x => x.CheckPasswordAsync(appUser, userDto.Password)).ReturnsAsync(false); // The password check fails
        
        var controller = new AuthController(mockUserManager.Object, GetMockConfiguration().Object);
        
        // Act
        var result = await controller.Login(userDto);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }
    #endregion
}