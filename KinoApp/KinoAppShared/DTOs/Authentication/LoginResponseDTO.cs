namespace KinoAppShared.DTOs.Authentication
{
    /// <summary>
    /// Result returned after a successful login or token refresh.
    /// </summary>
    public class LoginResponseDTO
    {
        /// <summary>
        /// Short-lived JWT access token.
        /// </summary>
        public TokenInfoDTO Token { get; set; } = null!;

        /// <summary>
        /// Long-lived JWT refresh token.
        /// </summary>
        public TokenInfoDTO RefreshToken { get; set; } = null!;

        /// <summary>
        /// User first name.
        /// </summary>
        public string Vorname { get; set; } = null!;

        /// <summary>
        /// User last name.
        /// </summary>
        public string Nachname { get; set; } = null!;

        /// <summary>
        /// User email address.
        /// </summary>
        public string Email { get; set; } = null!;

        /// <summary>
        /// Assigned application role (e.g. User, Admin).
        /// </summary>
        public string Role { get; set; } = "User";
    }
}
