using AutoMapper;
using KinoAppCore.Abstractions;
using KinoAppDB.Entities;         
using KinoAppDB.Repository;
using KinoAppShared.DTOs;

namespace KinoAppCore.Services;

public sealed class KundeService : IKundeService
{
    private readonly IKundeRepository _repo;
    private readonly IMapper _mapper;
    private readonly IPasswordHasher _hasher;

    public KundeService(IKundeRepository repo, IMapper mapper, IPasswordHasher hasher)
    {
        _repo = repo;
        _mapper = mapper;
        _hasher = hasher;
    }

    public async Task<FullKundeDTO?> GetAsync(long id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        return entity is null ? null : _mapper.Map<FullKundeDTO>(entity);
    }

    public async Task<IReadOnlyList<FullKundeDTO>> GetAllAsync(CancellationToken ct = default)
    {
        var list = await _repo.GetAllAsync(ct);
        return _mapper.Map<IReadOnlyList<FullKundeDTO>>(list);
    }

    public async Task<FullKundeDTO> CreateAsync(FullKundeDTO dto, CancellationToken ct = default)
    {
        var entity = _mapper.Map<KundeEntity>(dto);
        entity.Passwort = _hasher.Hash(dto.Passwort);
        await _repo.AddAsync(entity, ct);
        await _repo.SaveAsync(ct);
        return _mapper.Map<FullKundeDTO>(entity);
    }

    public async Task<FullKundeDTO?> UpdateAsync(long id, FullKundeDTO dto, CancellationToken ct = default)
    {
        var existing = await _repo.GetByIdAsync(id, ct);
        if (existing is null)
            return null;

        _mapper.Map(dto, existing);
        existing.Passwort = _hasher.Hash(dto.Passwort);
        await _repo.UpdateAsync(existing, ct);
        await _repo.SaveAsync(ct);

        return _mapper.Map<FullKundeDTO>(existing);
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken ct = default)
    {
        var existing = await _repo.GetByIdAsync(id, ct);
        if (existing is null)
            return false;

        await _repo.DeleteAsync(existing, ct);
        await _repo.SaveAsync(ct);
        return true;
    }
}
