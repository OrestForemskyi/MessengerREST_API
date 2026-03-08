using MessengerREST_API.Controllers;
using MessengerREST_API.Data;
using MessengerREST_API.DTOs;
using MessengerREST_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MessengerREST_API.Tests.UnitTests
{
    public class UsersControllerTests
    {
        //Віртуальна база даних
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact] 
        public async Task Register_NewUser_ReturnsOkResult()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new UsersController(context);
            var newUserDto = new RegisterUserDto { Username = "TestUser", Password = "password123" };

            // Act
            var result = await controller.Register(newUserDto);

            // Assert
            // Чи повернувся статус 200 OK
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            // Чи створений юзер
            var returnedUser = Assert.IsType<User>(okResult.Value);
            Assert.Equal("TestUser", returnedUser.Username);
        }

        [Fact]
        public async Task Register_ExistingUser_ReturnsBadRequest()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            context.Users.Add(new User { Username = "ExistingUser", PasswordHash = "hash" });
            await context.SaveChangesAsync();

            var controller = new UsersController(context);
            var duplicateUserDto = new RegisterUserDto { Username = "ExistingUser", Password = "newpassword" };

            // Act
            var result = await controller.Register(duplicateUserDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Користувач з таким іменем вже існує.", badRequestResult.Value);
        }
    }
}