namespace MessengerREST_API.DTOs
{
    public class SendMessageDto
    {
        public int ChatId { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}
