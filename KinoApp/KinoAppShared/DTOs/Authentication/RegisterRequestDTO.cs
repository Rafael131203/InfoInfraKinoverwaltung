namespace KinoAppShared.DTOs.Authentication
{
    public class RegisterRequestDTO
    {
        public string Vorname { get; set; } = string.Empty;
        public string Nachname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Passwort { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
    }
}
