namespace MessengerREST_API.Models
{
    public class Chat
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        // звязки
        public List<ChatUser> ChatUsers { get; set; } = new();
        public List<Message> Messages { get; set; } = new();
    }
}