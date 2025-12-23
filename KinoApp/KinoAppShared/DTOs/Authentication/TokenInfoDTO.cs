namespace KinoAppShared.DTOs.Authentication
{
    /// <summary>
    /// Metadata container for a generated JWT token.
    /// </summary>
    public class TokenInfoDTO
    {
        /// <summary>
        /// Serialized JWT token string.
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// UTC timestamp when the token was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// UTC timestamp when the token expires.
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Claims embedded into the token for debugging or inspection.
        /// </summary>
        public Dictionary<string, string>? Claims { get; set; }
    }
}
