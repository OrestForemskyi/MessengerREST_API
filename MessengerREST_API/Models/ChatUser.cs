namespace MessengerREST_API.Models
{
    public class ChatUser
    {
        public int Id { get; set; }

        public int ChatId { get; set; }
        public Chat Chat { get; set; } = null!;

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}