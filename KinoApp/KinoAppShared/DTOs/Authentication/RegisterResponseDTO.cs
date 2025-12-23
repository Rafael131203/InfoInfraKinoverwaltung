namespace KinoAppShared.DTOs.Authentication
{
    /// <summary>
    /// Result returned after a successful user registration.
    /// </summary>
    public class RegisterResponseDTO
    {
        /// <summary>
        /// Unique identifier of the newly created user.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// User first name.
        /// </summary>
        public string Vorname { get; set; } = string.Empty;

        /// <summary>
        /// User last name.
        /// </summary>
        public string Nachname { get; set; } = string.Empty;

        /// <summary>
        /// User email address.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Assigned application role.
        /// </summary>
        public string Role { get; set; } = string.Empty;
    }
}
