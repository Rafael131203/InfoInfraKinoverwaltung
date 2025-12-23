namespace KinoAppShared.DTOs.Authentication
{
    /// <summary>
    /// Request payload for user login.
    /// </summary>
    public class LoginRequestDTO
    {
        /// <summary>
        /// User email address used for authentication.
        /// </summary>
        public string Email { get; set; } = null!;

        /// <summary>
        /// Plain text password provided by the user.
        /// </summary>
        public string Passwort { get; set; } = null!;
    }
}
