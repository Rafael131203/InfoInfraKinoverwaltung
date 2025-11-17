namespace KinoAppShared.DTOs.Authentication
{
    public class RegisterResponseDTO
    {
        public long Id { get; set; }
        public string Vorname { get; set; } = string.Empty;
        public string Nachname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
