using MessengerREST_API.Data;
using MessengerREST_API.DTOs;
using MessengerREST_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MessengerREST_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ChatsController(AppDbContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }

        // GET /api/chats
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ChatResponseDto>>> GetAllChats()
        {
            var userId = GetCurrentUserId();

            var chats = await _context.Chats
                .Include(c => c.ChatUsers)
                .Where(c => c.ChatUsers.Any(cu => cu.UserId == userId))
                .Select(c => new ChatResponseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    ParticipantCount = c.ChatUsers.Count,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return Ok(chats);
        }

        // GET /api/chats/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ChatResponseDto>> GetChat(int id)
        {
            var userId = GetCurrentUserId();

            var chat = await _context.Chats
                .Include(c => c.ChatUsers)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (chat == null)
                return NotFound("Чат не знайдено.");

            var isParticipant = chat.ChatUsers.Any(cu => cu.UserId == userId);
            if (!isParticipant)
                return Forbid();

            var chatDto = new ChatResponseDto
            {
                Id = chat.Id,
                Name = chat.Name,
                ParticipantCount = chat.ChatUsers.Count,
                CreatedAt = chat.CreatedAt
            };

            return Ok(chatDto);
        }

        // POST /api/chats
        [HttpPost]
        public async Task<ActionResult<ChatResponseDto>> CreateChat([FromBody] CreateChatDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest("Назва чату не може бути пустою.");

            var userId = GetCurrentUserId();

            var newChat = new Chat
            {
                Name = request.Name,
                CreatedAt = DateTime.UtcNow
            };

            _context.Chats.Add(newChat);
            await _context.SaveChangesAsync();

            var chatUser = new ChatUser
            {
                ChatId = newChat.Id,
                UserId = userId,
                JoinedAt = DateTime.UtcNow
            };

            _context.ChatUsers.Add(chatUser);
            await _context.SaveChangesAsync();

            var chatDto = new ChatResponseDto
            {
                Id = newChat.Id,
                Name = newChat.Name,
                ParticipantCount = 1,
                CreatedAt = newChat.CreatedAt
            };

            return CreatedAtAction(nameof(GetChat), new { id = newChat.Id }, chatDto);
        }

        // PUT /api/chats/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateChat(int id, [FromBody] UpdateChatDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest("Назва чату не може бути пустою.");

            var userId = GetCurrentUserId();

            var chat = await _context.Chats
                .Include(c => c.ChatUsers)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (chat == null)
                return NotFound("Чат не знайдено.");

            var isParticipant = chat.ChatUsers.Any(cu => cu.UserId == userId);
            if (!isParticipant)
                return Forbid();

            chat.Name = request.Name;
            _context.Chats.Update(chat);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE /api/chats/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteChat(int id)
        {
            var userId = GetCurrentUserId();

            var chat = await _context.Chats
                .Include(c => c.ChatUsers)
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (chat == null)
                return NotFound("Чат не знайдено.");

            var isParticipant = chat.ChatUsers.Any(cu => cu.UserId == userId);
            if (!isParticipant)
                return Forbid();

            _context.Chats.Remove(chat);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST /api/chats/{chatId}/users/{userId}
        [HttpPost("{chatId}/users/{userId}")]
        public async Task<ActionResult> AddUserToChat(int chatId, int userId)
        {
            var currentUserId = GetCurrentUserId();

            var chat = await _context.Chats
                .Include(c => c.ChatUsers)
                .FirstOrDefaultAsync(c => c.Id == chatId);

            if (chat == null)
                return NotFound("Чат не знайдено.");

            var isParticipant = chat.ChatUsers.Any(cu => cu.UserId == currentUserId);
            if (!isParticipant)
                return Forbid();

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound("Користувача не знайдено.");

            var existingChatUser = await _context.ChatUsers
                .FirstOrDefaultAsync(cu => cu.ChatId == chatId && cu.UserId == userId);

            if (existingChatUser != null)
                return BadRequest("Користувач вже в цьому чаті.");

            var chatUser = new ChatUser
            {
                ChatId = chatId,
                UserId = userId,
                JoinedAt = DateTime.UtcNow
            };

            _context.ChatUsers.Add(chatUser);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Користувача додано до чату." });
        }

        // DELETE /api/chats/{chatId}/users/{userId}
        [HttpDelete("{chatId}/users/{userId}")]
        public async Task<ActionResult> RemoveUserFromChat(int chatId, int userId)
        {
            var currentUserId = GetCurrentUserId();

            var chat = await _context.Chats
                .Include(c => c.ChatUsers)
                .FirstOrDefaultAsync(c => c.Id == chatId);

            if (chat == null)
                return NotFound("Чат не знайдено.");

            var chatUser = await _context.ChatUsers
                .FirstOrDefaultAsync(cu => cu.ChatId == chatId && cu.UserId == userId);

            if (chatUser == null)
                return NotFound("Користувач не в цьому чаті.");

            if (currentUserId != userId && !chat.ChatUsers.Any(cu => cu.UserId == currentUserId))
                return Forbid();

            _context.ChatUsers.Remove(chatUser);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET /api/chats/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<ChatResponseDto>>> GetUserChats(int userId)
        {
            var chats = await _context.Chats
                .Include(c => c.ChatUsers)
                .Where(c => c.ChatUsers.Any(cu => cu.UserId == userId))
                .Select(c => new ChatResponseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    ParticipantCount = c.ChatUsers.Count,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return Ok(chats);
        }
    }
}
