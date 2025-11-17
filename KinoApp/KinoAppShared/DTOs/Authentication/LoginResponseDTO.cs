namespace KinoAppShared.DTOs.Authentication
{
    public class LoginResponseDTO
    {
        public TokenInfoDTO Token { get; set; } = null!;
        public TokenInfoDTO RefreshToken { get; set; } = null!;
        public string Vorname { get; set; } = null!;
        public string Nachname { get; set; } = null!;
        public string Email { get; set; } = null!;
    }
}
