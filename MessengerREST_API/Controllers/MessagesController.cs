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
    public class MessagesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MessagesController(AppDbContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }

        // GET /api/messages/chat/{chatId}
        [HttpGet("chat/{chatId}")]
        public async Task<ActionResult<IEnumerable<MessageResponseDto>>> GetChatMessages(int chatId)
        {
            var userId = GetCurrentUserId();

            var chat = await _context.Chats
                .Include(c => c.ChatUsers)
                .FirstOrDefaultAsync(c => c.Id == chatId);

            if (chat == null)
                return NotFound("Чат не знайдено.");

            var isParticipant = chat.ChatUsers.Any(cu => cu.UserId == userId);
            if (!isParticipant)
                return Forbid();

            var messages = await _context.Messages
                .Include(m => m.Sender)
                .Where(m => m.ChatId == chatId)
                .OrderBy(m => m.CreatedAt)
                .Select(m => new MessageResponseDto
                {
                    Id = m.Id,
                    ChatId = m.ChatId,
                    SenderId = m.SenderId,
                    SenderUsername = m.Sender.Username,
                    Content = m.Content,
                    CreatedAt = m.CreatedAt
                })
                .ToListAsync();

            return Ok(messages);
        }

        // GET /api/messages/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<MessageResponseDto>> GetMessage(int id)
        {
            var userId = GetCurrentUserId();

            var message = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Chat)
                .ThenInclude(c => c.ChatUsers)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (message == null)
                return NotFound("Повідомлення не знайдено.");

            var isParticipant = message.Chat.ChatUsers.Any(cu => cu.UserId == userId);
            if (!isParticipant)
                return Forbid();

            var messageDto = new MessageResponseDto
            {
                Id = message.Id,
                ChatId = message.ChatId,
                SenderId = message.SenderId,
                SenderUsername = message.Sender.Username,
                Content = message.Content,
                CreatedAt = message.CreatedAt
            };

            return Ok(messageDto);
        }

        // POST /api/messages
        [HttpPost]
        public async Task<ActionResult<MessageResponseDto>> SendMessage([FromBody] SendMessageDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Content))
                return BadRequest("Зміст повідомлення не може бути пустим.");

            var userId = GetCurrentUserId();

            var chat = await _context.Chats
                .Include(c => c.ChatUsers)
                .FirstOrDefaultAsync(c => c.Id == request.ChatId);

            if (chat == null)
                return NotFound("Чат не знайдено.");

            var isParticipant = chat.ChatUsers.Any(cu => cu.UserId == userId);
            if (!isParticipant)
                return Forbid();

            var message = new Message
            {
                ChatId = request.ChatId,
                SenderId = userId,
                Content = request.Content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            await _context.Entry(message).Reference(m => m.Sender).LoadAsync();

            var messageDto = new MessageResponseDto
            {
                Id = message.Id,
                ChatId = message.ChatId,
                SenderId = message.SenderId,
                SenderUsername = message.Sender.Username,
                Content = message.Content,
                CreatedAt = message.CreatedAt
            };

            return CreatedAtAction(nameof(GetMessage), new { id = message.Id }, messageDto);
        }

        // PUT /api/messages/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateMessage(int id, [FromBody] UpdateMessageDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Content))
                return BadRequest("Зміст повідомлення не може бути пустим.");

            var userId = GetCurrentUserId();

            var message = await _context.Messages.FirstOrDefaultAsync(m => m.Id == id);

            if (message == null)
                return NotFound("Повідомлення не знайдено.");

            if (message.SenderId != userId)
                return Forbid();

            message.Content = request.Content;
            _context.Messages.Update(message);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE /api/messages/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var userId = GetCurrentUserId();

            var message = await _context.Messages
                .Include(m => m.Chat)
                .ThenInclude(c => c.ChatUsers)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (message == null)
                return NotFound("Повідомлення не знайдено.");

            if (message.SenderId != userId)
            {
                var isAdmin = message.Chat.ChatUsers.Any(cu => cu.UserId == userId);
                if (!isAdmin)
                    return Forbid();
            }

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
