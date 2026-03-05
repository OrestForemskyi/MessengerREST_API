using MessengerREST_API.Data;
using MessengerREST_API.DTOs;
using MessengerREST_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MessengerREST_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        // Підключив базу даних через конструктор
        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // /api/users/register
        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(RegisterUserDto request)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                return BadRequest("Користувач з таким іменем вже існує.");
            }

            var newUser = new User
            {
                Username = request.Username,
                PasswordHash = request.Password,
                CreatedAt = DateTime.UtcNow
            };

  
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(newUser);
        }

        // /api/users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(users);
        }

        // /api/users/{id}
        [HttpGet("{id}")]
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