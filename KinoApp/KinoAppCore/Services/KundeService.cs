using AutoMapper;
using KinoAppCore.Abstractions;
using KinoAppCore.Entities;
using KinoAppShared.DTOs;

namespace KinoAppCore.Services;

public sealed class KundeService : IKundeService
{
    private readonly IKundeRepository _repo;
    private readonly IDbContextScope _scope;
    private readonly IMapper _mapper;

    public KundeService(IKundeRepository repo, IDbContextScope scope, IMapper mapper)
    {
        _repo = repo;
        _scope = scope;
        _mapper = mapper;
    }

    public async Task<FullKundeDTO?> GetAsync(long id, CancellationToken ct = default)
    {
        var e = await _repo.GetByIdAsync(id, ct);
        return e is null ? null : _mapper.Map<FullKundeDTO>(e);
    }

    public async Task<IReadOnlyList<FullKundeDTO>> GetAllAsync(CancellationToken ct = default)
    {
        var list = await _repo.GetAllAsync(ct);
        return _mapper.Map<IReadOnlyList<FullKundeDTO>>(list);
    }

    public async Task<FullKundeDTO> CreateAsync(FullKundeDTO dto, CancellationToken ct = default)
    {
        _scope.Create();
        await _scope.BeginAsync(ct);
        try
        {
            var entity = _mapper.Map<Kunde>(dto);
            await _repo.AddAsync(entity, ct);
            await _repo.SaveAsync(ct);
            await _scope.CommitAsync(ct);
            return _mapper.Map<FullKundeDTO>(entity);
        }
        catch
        {
            await _scope.RollbackAsync();
            throw;
        }
    }

    public async Task<FullKundeDTO?> UpdateAsync(long id, FullKundeDTO dto, CancellationToken ct = default)
    {
        _scope.Create();
        await _scope.BeginAsync(ct);
        try
        {
            var existing = await _repo.GetByIdAsync(id, ct);
            if (existing is null)
            {
                await _scope.RollbackAsync();
                return null;
            }
            _mapper.Map(dto, existing);
            await _repo.UpdateAsync(existing, ct);
            await _repo.SaveAsync(ct);
            await _scope.CommitAsync(ct);
            return _mapper.Map<FullKundeDTO>(existing);
        }
        catch
        {
            await _scope.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken ct = default)
    {
        _scope.Create();
        await _scope.BeginAsync(ct);
        try
        {
            var existing = await _repo.GetByIdAsync(id, ct);
            if (existing is null)
            {
                await _scope.RollbackAsync();
                return false;
            }
            await _repo.DeleteAsync(existing, ct);
            await _repo.SaveAsync(ct);
            await _scope.CommitAsync(ct);
            return true;
        }
        catch
        {
            await _scope.RollbackAsync();
            throw;
        }
    }

    public object DeleteAsync(Kunde entity)
    {
        throw new NotImplementedException();
    }
}
