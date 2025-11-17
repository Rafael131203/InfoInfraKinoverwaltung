using KinoAppCore.Abstractions;
using KinoAppDB.Repository;
using KinoAppShared.DTOs.Authentication;

namespace KinoAppCore.Services
{
    public class LoginService : ILoginService
    {
        private readonly IKundeRepository _repo;
        private readonly ITokenService _tokenService;
        private readonly IPasswordHasher _hasher;

        public LoginService(IKundeRepository repo, ITokenService tokenService, IPasswordHasher hasher)
        {
            _repo = repo;
            _tokenService = tokenService;
            _hasher = hasher;
        }

        public async Task<LoginResponseDTO?> AuthenticateAsync(LoginRequestDTO request, CancellationToken ct = default)
        {
            var kunde = await _repo.FindByEmailAsync(request.Email, ct);
            if (kunde == null) return null;

            if (!_hasher.Verify(request.Passwort, kunde.Passwort))
                return null;

            var access = _tokenService.GenerateAccessToken(kunde.Id, kunde.Email, kunde.Vorname, kunde.Nachname);
            var refresh = _tokenService.GenerateRefreshToken(kunde.Id, kunde.Email);

            return new LoginResponseDTO
            {
                Token = _tokenService.GenerateAccessToken(kunde.Id, kunde.Email, kunde.Vorname, kunde.Nachname),
                RefreshToken = _tokenService.GenerateRefreshToken(kunde.Id, kunde.Email),
                Email = kunde.Email,
                Vorname = kunde.Vorname,
                Nachname = kunde.Nachname
            };
        }

        public async Task<LoginResponseDTO?> RefreshAsync(string refreshToken, CancellationToken ct = default)
        {
            var payload = _tokenService.ValidateRefreshToken(refreshToken);
            if (payload is null)
                return null;

            var (userId, email) = payload.Value;

            var kunde = await _repo.GetByIdAsync(userId, ct);
            if (kunde == null || !string.Equals(kunde.Email, email, StringComparison.OrdinalIgnoreCase))
                return null;

            var newAccess = _tokenService.GenerateAccessToken(kunde.Id, kunde.Email, kunde.Vorname, kunde.Nachname);
            var newRefresh = _tokenService.GenerateRefreshToken(kunde.Id, kunde.Email);

            return new LoginResponseDTO
            {
                Token = newAccess,
                RefreshToken = newRefresh,
                Email = kunde.Email,
                Vorname = kunde.Vorname,
                Nachname = kunde.Nachname
            };
        }
    }
}
