using KinoAppShared.DTOs.Authentication;

namespace KinoAppCore.Abstractions
{
    /// <summary>
    /// Provides functionality for generating and validating Jwt tokens.
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Generates a new access token for the specified user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="email">The user's email address.</param>
        /// <param name="vorname">The user's first name.</param>
        /// <param name="nachname">The user's last name.</param>
        /// <param name="role">The user's assigned role.</param>
        /// <returns>
        /// A <see cref="TokenInfoDTO"/> containing the generated access token and related metadata.
        /// </returns>
        TokenInfoDTO GenerateAccessToken(long userId, string email, string vorname, string nachname, string role
        );

        /// <summary>
        /// Generates a new refresh token for the specified user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="email">The user's email address.</param>
        /// <returns>
        /// A <see cref="TokenInfoDTO"/> containing the generated refresh token and related metadata.
        /// </returns>
        TokenInfoDTO GenerateRefreshToken(long userId, string email);

        /// <summary>
        /// Validates a refresh token and extracts the associated user information.
        /// </summary>
        /// <param name="refreshToken">The refresh token to validate.</param>
        /// <returns>
        /// A tuple containing the user ID and email if the token is valid;
        /// otherwise <c>null</c>.
        /// </returns>
        (long userId, string email)? ValidateRefreshToken(string refreshToken);
    }
}
