namespace KinoAppShared.DTOs.Authentication
{
    /// <summary>
    /// Request payload for registering a new user account.
    /// </summary>
    public class RegisterRequestDTO
    {
        /// <summary>
        /// User first name.
        /// </summary>
        public string Vorname { get; set; } = string.Empty;

        /// <summary>
        /// User last name.
        /// </summary>
        public string Nachname { get; set; } = string.Empty;

        /// <summary>
        /// User email address (must be unique).
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Plain text password that will be hashed before storage.
        /// </summary>
        public string Passwort { get; set; } = string.Empty;

        /// <summary>
        /// Role assigned to the user.
        /// </summary>
        public string Role { get; set; } = "User";
    }
}
