namespace KinoAppShared.DTOs.Authentication
{
    public class LoginRequestDTO
    {
        public string Email { get; set; } = null!;
        public string Passwort { get; set; } = null!;
    }
}
