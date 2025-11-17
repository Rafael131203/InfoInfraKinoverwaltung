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
    }
}
