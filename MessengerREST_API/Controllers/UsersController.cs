using MessengerREST_API.Data;
using MessengerREST_API.DTOs;
using MessengerREST_API.Models;
using MessengerREST_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MessengerREST_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IJwtService _jwtService;

        public UsersController(AppDbContext context, IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        // /api/users/register
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterUserDto request)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                return BadRequest("Користувач з таким іменем вже існує.");
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var newUser = new User
            {
                Username = request.Username,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            var token = _jwtService.GenerateToken(newUser.Id, newUser.Username);
            var expiresAt = _jwtService.GetTokenExpirationTime();

            return Ok(new AuthResponseDto
            {
                Id = newUser.Id,
                Username = newUser.Username,
                Token = token,
                ExpiresAt = expiresAt
            });
        }

        // /api/users/login
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginUserDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized("Неправильне ім'я користувача або пароль.");
            }

            var token = _jwtService.GenerateToken(user.Id, user.Username);
            var expiresAt = _jwtService.GetTokenExpirationTime();

            return Ok(new AuthResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                Token = token,
                ExpiresAt = expiresAt
            });
        }

        // /api/users
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(users);
        }

        // /api/users/{id}
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound("Користувача не знайдено.");
            }

            return Ok(user);
        }
    }
}