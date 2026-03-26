namespace MessengerREST_API.DTOs
{
    public class ChatResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int ParticipantCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
