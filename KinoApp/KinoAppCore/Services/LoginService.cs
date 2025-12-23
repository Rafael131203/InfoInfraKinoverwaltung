using AutoMapper;
using KinoAppCore.Abstractions;
using KinoAppDB.Entities;
using KinoAppDB.Repository;
using KinoAppShared.DTOs.Authentication;
using KinoAppShared.Messaging;

namespace KinoAppCore.Services
{
    /// <summary>
    /// Default implementation of <see cref="ILoginService"/> using repository-backed user storage and token services.
    /// </summary>
    /// <remarks>
    /// Password hashing is delegated to <see cref="IPasswordHasher"/> and token issuance/validation is delegated to
    /// <see cref="ITokenService"/>. A successful registration publishes a <see cref="KundeRegistered"/> event.
    /// </remarks>
    public class LoginService : ILoginService
    {
        private readonly IUserRepository _repo;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly IPasswordHasher _hasher;
        private readonly IMessageBus _bus;

        /// <summary>
        /// Creates a new <see cref="LoginService"/>.
        /// </summary>
        public LoginService(
            IUserRepository repo,
            ITokenService tokenService,
            IPasswordHasher hasher,
            IMapper mapper,
            IMessageBus bus)
        {
            _repo = repo;
            _tokenService = tokenService;
            _hasher = hasher;
            _mapper = mapper;
            _bus = bus;
        }

        /// <inheritdoc />
        public async Task<LoginResponseDTO?> AuthenticateAsync(LoginRequestDTO request, CancellationToken ct = default)
        {
            var kunde = await _repo.FindByEmailAsync(request.Email, ct);
            if (kunde == null)
                return null;

            if (!_hasher.Verify(request.Passwort, kunde.Passwort))
                return null;

            var access = _tokenService.GenerateAccessToken(kunde.Id, kunde.Email, kunde.Vorname, kunde.Nachname, kunde.Role);
            var refresh = _tokenService.GenerateRefreshToken(kunde.Id, kunde.Email);

            return new LoginResponseDTO
            {
                Token = access,
                RefreshToken = refresh,
                Email = kunde.Email,
                Vorname = kunde.Vorname,
                Nachname = kunde.Nachname,
                Role = kunde.Role
            };
        }

        /// <inheritdoc />
        public async Task<LoginResponseDTO?> RefreshAsync(string refreshToken, CancellationToken ct = default)
        {
            var payload = _tokenService.ValidateRefreshToken(refreshToken);
            if (payload is null)
                return null;

            var (userId, email) = payload.Value;

            var kunde = await _repo.GetByIdAsync(userId, ct);
            if (kunde == null || !string.Equals(kunde.Email, email, StringComparison.OrdinalIgnoreCase))
                return null;

            var newAccess = _tokenService.GenerateAccessToken(kunde.Id, kunde.Email, kunde.Vorname, kunde.Nachname, kunde.Role);
            var newRefresh = _tokenService.GenerateRefreshToken(kunde.Id, kunde.Email);

            return new LoginResponseDTO
            {
                Token = newAccess,
                RefreshToken = newRefresh,
                Email = kunde.Email,
                Vorname = kunde.Vorname,
                Nachname = kunde.Nachname,
                Role = kunde.Role
            };
        }

        /// <inheritdoc />
        public async Task<RegisterResponseDTO> RegisterAsync(RegisterRequestDTO dto, CancellationToken ct = default)
        {
            var existing = await _repo.FindByEmailAsync(dto.Email);
            if (existing != null)
                throw new InvalidOperationException("Email already registered.");

            var entity = _mapper.Map<UserEntity>(dto);

            entity.Passwort = _hasher.Hash(dto.Passwort);

            await _repo.AddAsync(entity, ct);
            await _repo.SaveAsync(ct);

            var @event = new KundeRegistered(
                entity.Id,
                entity.Email,
                entity.Vorname,
                entity.Nachname,
                DateTime.UtcNow,
                entity.Role
            );

            await _bus.PublishAsync(@event, ct);

            return _mapper.Map<RegisterResponseDTO>(entity);
        }
    }
}
