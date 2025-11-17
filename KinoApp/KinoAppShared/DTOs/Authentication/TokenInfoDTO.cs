namespace KinoAppShared.DTOs.Authentication
{
    public class TokenInfoDTO
    {
        public string Token { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public Dictionary<string, string>? Claims { get; set; }
    }
}
