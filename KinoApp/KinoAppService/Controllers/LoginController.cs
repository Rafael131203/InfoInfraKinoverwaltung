using KinoAppCore.Services;
using KinoAppDB;
using KinoAppShared.DTOs.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KinoAppService.Controllers
{
    /// <summary>
    /// Authentication endpoints for login, token refresh, and user registration.
    /// </summary>
    /// <remarks>
    /// The API issues access and refresh tokens. Logout is handled client-side by discarding tokens.
    /// </remarks>
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : BaseController
    {
        private readonly ILoginService _loginService;

        /// <summary>
        /// Creates a new <see cref="LoginController"/>.
        /// </summary>
        /// <param name="loginService">Service used to authenticate and register users.</param>
        /// <param name="scope">Database scope used for transactional execution.</param>
        public LoginController(ILoginService loginService, IKinoAppDbContextScope scope)
            : base(scope)
        {
            _loginService = loginService;
        }

        /// <summary>
        /// Authenticates a user using email and password and returns access/refresh tokens.
        /// </summary>
        /// <param name="request">Login request.</param>
        /// <param name="ct">Cancellation token.</param>
        [AllowAnonymous]
        [HttpPost]
        public Task<IActionResult> Login([FromBody] LoginRequestDTO request, CancellationToken ct) =>
            ExecuteAsync(async token =>
            {
                if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Passwort))
                    return new BadRequestObjectResult("Email and password are required.");

                var result = await _loginService.AuthenticateAsync(request, token);
                if (result == null)
                    return new UnauthorizedObjectResult("Invalid email or password.");

                return new OkObjectResult(result);
            }, ct);

        /// <summary>
        /// Exchanges a refresh token for a new access/refresh token pair.
        /// </summary>
        /// <param name="request">Refresh request containing the refresh token.</param>
        /// <param name="ct">Cancellation token.</param>
        [Authorize]
        [HttpPost("refresh")]
        public Task<IActionResult> Refresh([FromBody] RefreshRequestDTO request, CancellationToken ct) =>
            ExecuteAsync(async token =>
            {
                if (string.IsNullOrWhiteSpace(request.RefreshToken))
                    return new BadRequestObjectResult("Refresh token is required.");

                var result = await _loginService.RefreshAsync(request.RefreshToken, token);
                if (result == null)
                    return new UnauthorizedObjectResult("Invalid or expired refresh token.");

                return new OkObjectResult(result);
            }, ct);

        /// <summary>
        /// Registers a new user account.
        /// </summary>
        /// <param name="dto">Registration request.</param>
        /// <param name="ct">Cancellation token.</param>
        [AllowAnonymous]
        [HttpPost("register")]
        public Task<IActionResult> Register([FromBody] RegisterRequestDTO dto, CancellationToken ct) =>
            ExecuteAsync(async token =>
            {
                if (dto == null ||
                    string.IsNullOrWhiteSpace(dto.Email) ||
                    string.IsNullOrWhiteSpace(dto.Passwort))
                {
                    return new BadRequestObjectResult("Email and password are required.");
                }

                var allowedRoles = new[] { "User", "Admin" };
                if (!allowedRoles.Contains(dto.Role, StringComparer.OrdinalIgnoreCase))
                    return new BadRequestObjectResult("Invalid role. Allowed roles: User, Admin.");

                try
                {
                    var result = await _loginService.RegisterAsync(dto, token);
                    return new CreatedResult("/api/auth/register", result);
                }
                catch (InvalidOperationException ex)
                {
                    return new ConflictObjectResult(ex.Message);
                }
            }, ct);

        /// <summary>
        /// Logout endpoint for symmetry with client flows.
        /// </summary>
        /// <remarks>
        /// With stateless JWTs there is nothing to revoke server-side in this project. Clients log out by deleting
        /// locally stored tokens. If a refresh-token store is introduced later, this endpoint can revoke tokens.
        /// </remarks>
        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return NoContent();
        }
    }
}
