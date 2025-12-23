namespace KinoAppShared.DTOs.Authentication
{
    /// <summary>
    /// Request payload for refreshing an access token.
    /// </summary>
    public class RefreshRequestDTO
    {
        /// <summary>
        /// Previously issued refresh token.
        /// </summary>
        public string RefreshToken { get; set; } = null!;
    }
}
