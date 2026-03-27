namespace MessengerREST_API.DTOs
{
    public class AuthResponseDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
