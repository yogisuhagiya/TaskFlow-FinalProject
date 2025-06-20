// File: TaskFlow.Tests/CategoriesControllerTests.cs
// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using System.Security.Claims;
// using TaskFlow.Api.Controllers;
// using TaskFlow.Core.Models;
// using TaskFlow.Data;
// using Xunit;

// public class CategoriesControllerTests
// {
//     private ApplicationDbContext _context;
//     private CategoriesController _controller;
//     private readonly string _testUserId = "user-id-1";

//     private void SetupController()
//     {
//         var options = new DbContextOptionsBuilder<ApplicationDbContext>()
//             .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
//         _context = new ApplicationDbContext(options);
        
//         _context.Categories.Add(new Category { Id = 1, Name = "Work", AppUserId = _testUserId });
//         _context.SaveChanges();
        
//         _controller = new CategoriesController(_context);
//         var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.NameIdentifier, _testUserId) }));
//         _controller.ControllerContext = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user } };
//     }

//     [Fact]
//     public async Task GetCategories_ReturnsOnlyUserCategories()
//     {
//         SetupController();
//         var result = await _controller.GetCategories();
//         var categories = Assert.IsAssignableFrom<IEnumerable<Category>>(Assert.IsType<OkObjectResult>(result.Result).Value);
//         Assert.Single(categories);
//     }

//     [Fact]
//     public async Task CreateCategory_AssignsCorrectUserId()
//     {
//         SetupController();
//         var newCategory = new Category { Name = "Personal" };
//         var result = await _controller.CreateCategory(newCategory);
//         var category = Assert.IsType<Category>(Assert.IsType<CreatedAtActionResult>(result.Result).Value);
//         Assert.Equal(_testUserId, category.AppUserId);
//     }
// }

// File: TaskFlow.Tests/CategoriesControllerTests.cs
// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using System.Security.Claims;
// using TaskFlow.Api.Controllers;
// using TaskFlow.Core.Models;
// using TaskFlow.Data;
// using Xunit;

// public class CategoriesControllerTests
// {
//     // These are now initialized in the constructor, so the compiler is happy.
//     private readonly ApplicationDbContext _context;
//     private readonly CategoriesController _controller;
//     private readonly string _testUserId = "user-id-1";

//     // The setup logic has been moved from the separate method into the constructor.
//     public CategoriesControllerTests()
//     {
//         var options = new DbContextOptionsBuilder<ApplicationDbContext>()
//             .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
//         _context = new ApplicationDbContext(options);

//         // Seed the database with some initial data for the tests.
//         _context.Categories.Add(new Category { Id = 1, Name = "Work", AppUserId = _testUserId });
//         _context.SaveChanges();

//         // Create the controller instance we are testing.
//         _controller = new CategoriesController(_context);

//         // Mock a logged-in user.
//         var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.NameIdentifier, _testUserId) }));
//         _controller.ControllerContext = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user } };
//     }

//     [Fact]
//     public async Task GetCategories_ReturnsOnlyUserCategories()
//     {
//         // No need to call SetupController() anymore.
//         var result = await _controller.GetCategories();
//         var okResult = Assert.IsType<OkObjectResult>(result.Result);
//         var categories = Assert.IsAssignableFrom<IEnumerable<Category>>(okResult.Value);
//         Assert.Single(categories);
//     }

//     [Fact]
//     public async Task CreateCategory_AssignsCorrectUserId()
//     {
//         // No need to call SetupController() anymore.
//         var newCategory = new Category { Name = "Personal" };
//         var result = await _controller.CreateCategory(newCategory);
//         var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
//         var category = Assert.IsType<Category>(createdResult.Value);
//         Assert.Equal(_testUserId, category.AppUserId);
//     }
    
// }


// File: TaskFlow.Tests/CategoriesControllerTests.cs
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskFlow.Api.Controllers;
using TaskFlow.Core.Models;
using TaskFlow.Data;
using Xunit;

public class CategoriesControllerTests
{
    private readonly ApplicationDbContext _context;
    private readonly CategoriesController _controller;
    private readonly string _testUserId = "user-id-1";
    private readonly string _otherUserId = "user-id-2";

    // The setup logic is now in the constructor to satisfy C# nullability rules.
    public CategoriesControllerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
        _context = new ApplicationDbContext(options);
        
        // Seed the database with initial data.
        _context.Categories.AddRange(
            new Category { Id = 1, Name = "Work", AppUserId = _testUserId },
            new Category { Id = 2, Name = "Home", AppUserId = _testUserId },
            new Category { Id = 3, Name = "Secret", AppUserId = _otherUserId }
        );
        _context.SaveChanges();
        
        _controller = new CategoriesController(_context);
        
        // Mock a logged-in user.
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.NameIdentifier, _testUserId) }));
        _controller.ControllerContext = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user } };
    }

    [Fact]
    public async Task GetCategories_ReturnsOnlyUserCategories()
    {
        var result = await _controller.GetCategories();
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var categories = Assert.IsAssignableFrom<IEnumerable<Category>>(okResult.Value);
        Assert.Equal(2, categories.Count()); // User 1 should have 2 categories.
    }

    [Fact]
    public async Task CreateCategory_AssignsCorrectUserIdAndReturnsCategory()
    {
        var newCategoryDto = new Category { Name = "New Project" };
        var result = await _controller.CreateCategory(newCategoryDto);
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var category = Assert.IsType<Category>(createdResult.Value);
        Assert.Equal(_testUserId, category.AppUserId);
        Assert.Equal("New Project", category.Name);
    }

    [Fact]
    public async Task DeleteCategory_WithValidId_ReturnsNoContent()
    {
        var result = await _controller.DeleteCategory(1);
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteCategory_WithValidId_ActuallyDeletesCategory()
    {
        await _controller.DeleteCategory(1);
        var deletedCategory = await _context.Categories.FindAsync(1);
        Assert.Null(deletedCategory);
    }

    [Fact]
    public async Task DeleteCategory_WithInvalidId_ReturnsNotFound()
    {
        var result = await _controller.DeleteCategory(999);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteCategory_WithIdOfAnotherUser_ReturnsNotFound()
    {
        // User 1 tries to delete Category 3, which belongs to User 2
        var result = await _controller.DeleteCategory(3);
        Assert.IsType<NotFoundResult>(result);
    }
}