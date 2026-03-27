using MessengerREST_API.Data;
using MessengerREST_API.DTOs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;
using DotNetEnv;

namespace MessengerREST_API.Tests.IntegrationTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        public CustomWebApplicationFactory()
        {
            // Find .env
            var envPath = FindEnvFile();
            
            if (string.IsNullOrEmpty(envPath))
            {
                throw new InvalidOperationException(
                    "Could not find .env file. " +
                    "Please ensure .env file exists in MessengerREST_API directory."
                );
            }

            DotNetEnv.Env.Load(envPath);
        }

        private static string FindEnvFile()
        {
         
            var currentDir = new DirectoryInfo(AppContext.BaseDirectory);

            while (currentDir != null)
            {
                var envPath = Path.Combine(currentDir.FullName, ".env");
                if (File.Exists(envPath))
                {
                    return envPath;
                }

                var projectEnvPath = Path.Combine(currentDir.FullName, "MessengerREST_API", ".env");
                if (File.Exists(projectEnvPath))
                {
                    return projectEnvPath;
                }

                currentDir = currentDir.Parent;

                if (currentDir?.Parent == null)
                {
                    break;
                }
            }

            return null;
        }

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
            // Create Client
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetAllUsers_WithoutToken_ReturnUnauthorized()
        {
            var response = await _client.GetAsync("/api/users");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
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

            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            Assert.NotNull(authResponse.Token);
            Assert.Equal("IntegrationTestUser", authResponse.Username);
        }

        [Fact]
        public async Task LoginUser_WithValidCredentials_ReturnsToken()
        {
            // Register a new user first
            var newUser = new RegisterUserDto
            {
                Username = "LoginTestUser",
                Password = "TestPassword123"
            };

            var registerResponse = await _client.PostAsJsonAsync("/api/users/register", newUser);
            var registerAuthResponse = await registerResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

            // Login with details of new user
            var loginRequest = new LoginUserDto
            {
                Username = "LoginTestUser",
                Password = "TestPassword123"
            };

            var loginResponse = await _client.PostAsJsonAsync("/api/users/login", loginRequest);

            Assert.True(loginResponse.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

            var loginAuthResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
            Assert.NotNull(loginAuthResponse.Token);
            Assert.Equal("LoginTestUser", loginAuthResponse.Username);
        }

        [Fact]
        public async Task GetAllUsers_WithValidToken_ReturnsSuccess()
        {
            // Register and login to get token
            var newUser = new RegisterUserDto
            {
                Username = "AuthorizedUser",
                Password = "AuthPassword123"
            };

            var registerResponse = await _client.PostAsJsonAsync("/api/users/register", newUser);
            var authResponse = await registerResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

            // Add token to client
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse.Token);

            // Make request with token
            var response = await _client.GetAsync("/api/users");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}