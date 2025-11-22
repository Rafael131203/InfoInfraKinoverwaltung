using KinoAppCore.Services;
using KinoAppDB;
using KinoAppShared.DTOs.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace KinoAppService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : BaseController
    {
        private readonly ILoginService _loginService;

        public LoginController(ILoginService loginService, IKinoAppDbContextScope scope)
            : base(scope)
        {
            _loginService = loginService;
        }

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

                // ONLY allow User or Admin
                var allowedRoles = new[] { "User", "Admin" };
                if (!allowedRoles.Contains(dto.Role, StringComparer.OrdinalIgnoreCase))
                {
                    return new BadRequestObjectResult("Invalid role. Allowed roles: User, Admin.");
                }

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


        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // With stateless JWT there is nothing to revoke server-side in this project.
            // "Logout" is handled by the client deleting its tokens.
            // If you later add a refresh-token store, you can mark tokens as revoked here.
            return NoContent();
        }
    }
}
