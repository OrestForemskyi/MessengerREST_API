using MessengerREST_API.Data;
using MessengerREST_API.DTOs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace MessengerREST_API.Tests.IntegrationTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {

            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase("IntegrationTestDb");
                });
            });
        }
    }

    public class UsersIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public UsersIntegrationTests(CustomWebApplicationFactory factory)
        {
            // Створює клієнта
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetAllUsers_EndpointReturnsSuccess()
        {
            var response = await _client.GetAsync("/api/users");

            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task RegisterUser_WithValidData_ReturnsSuccess()
        {
            var newUser = new RegisterUserDto
            {
                Username = "IntegrationTestUser",
                Password = "SuperPassword123"
            };

            var response = await _client.PostAsJsonAsync("/api/users/register", newUser);

            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}