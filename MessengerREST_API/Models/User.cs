namespace MessengerREST_API.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        //Relationships
        public List<ChatUser> ChatUsers { get; set; } = new();
        public List<Message> Messages { get; set; } = new();
    }
}