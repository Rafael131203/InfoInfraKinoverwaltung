using KinoAppShared.DTOs;

namespace KinoAppCore.Services;

public interface IKundeService
{
    Task<FullKundeDTO?> GetAsync(long id, CancellationToken ct = default);
    Task<IReadOnlyList<FullKundeDTO>> GetAllAsync(CancellationToken ct = default);
    Task<FullKundeDTO> CreateAsync(FullKundeDTO dto, CancellationToken ct = default);
    Task<FullKundeDTO?> UpdateAsync(long id, FullKundeDTO dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(long id, CancellationToken ct = default);
}
