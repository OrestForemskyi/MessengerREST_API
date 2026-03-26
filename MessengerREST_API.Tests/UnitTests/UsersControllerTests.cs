using MessengerREST_API.Controllers;
using MessengerREST_API.Data;
using MessengerREST_API.DTOs;
using MessengerREST_API.Models;
using MessengerREST_API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MessengerREST_API.Tests.UnitTests
{
    public class MockJwtService : IJwtService
    {
        public string GenerateToken(int userId, string username)
        {
            return "mock-jwt-token";
        }

        public DateTime GetTokenExpirationTime()
        {
            return DateTime.UtcNow.AddHours(24);
        }
    }

    public class UsersControllerTests
    {
        //Virtual database context and JWT service for testing
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        private IJwtService GetMockJwtService()
        {
            return new MockJwtService();
        }

        [Fact] 
        public async Task Register_NewUser_ReturnsOkResult()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var mockJwtService = GetMockJwtService();
            var controller = new UsersController(context, mockJwtService);
            var newUserDto = new RegisterUserDto { Username = "TestUser", Password = "password123" };

            // Act
            var result = await controller.Register(newUserDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedAuth = Assert.IsType<AuthResponseDto>(okResult.Value);
            Assert.Equal("TestUser", returnedAuth.Username);
            Assert.Equal("mock-jwt-token", returnedAuth.Token);
        }

        [Fact]
        public async Task Register_ExistingUser_ReturnsBadRequest()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            context.Users.Add(new User { Username = "ExistingUser", PasswordHash = BCrypt.Net.BCrypt.HashPassword("hash") });
            await context.SaveChangesAsync();

            var mockJwtService = GetMockJwtService();
            var controller = new UsersController(context, mockJwtService);
            var duplicateUserDto = new RegisterUserDto { Username = "ExistingUser", Password = "newpassword" };

            // Act
            var result = await controller.Register(duplicateUserDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Користувач з таким іменем вже існує.", badRequestResult.Value);
        }
    }
}