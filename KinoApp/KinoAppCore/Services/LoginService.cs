using AutoMapper;
using KinoAppCore.Abstractions;
using KinoAppDB.Entities;
using KinoAppDB.Repository;
using KinoAppShared.DTOs.Authentication;

namespace KinoAppCore.Services
{
    public class LoginService : ILoginService
    {
        private readonly IKundeRepository _repo;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly IPasswordHasher _hasher;

        public LoginService(IKundeRepository repo, ITokenService tokenService, IPasswordHasher hasher, IMapper mapper)
        {
            _repo = repo;
            _tokenService = tokenService;
            _hasher = hasher;
            _mapper = mapper;
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

        public async Task<RegisterResponseDTO> RegisterAsync(RegisterRequestDTO dto, CancellationToken ct = default)
        {
            // 1. Email check
            var existing = await _repo.FindByEmailAsync(dto.Email);
            if (existing != null)
                throw new InvalidOperationException("Email already registered.");

            // 2. Map → Kunde entity
            var entity = _mapper.Map<KundeEntity>(dto);

            // 3. Hash password AFTER mapping
            entity.Passwort = _hasher.Hash(dto.Passwort);

            // 4. Save to DB
            await _repo.AddAsync(entity, ct);
            await _repo.SaveAsync(ct);

            // 5. Map → DTO response
            return _mapper.Map<RegisterResponseDTO>(entity);
        }
    }
}
