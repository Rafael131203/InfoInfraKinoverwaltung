using KinoAppDB.Entities;
using KinoAppShared.DTOs.Kinosaal;

namespace KinoAppCore.Services
{
    public interface IKinosaalService
    {
        Task<long> CreateAsync(CreateKinosaalDTO kinosaal, int AnzahlSitzreihen, int GrößeSitzreihen, CancellationToken ct = default);
        Task<KinosaalDTO?> GetKinosaalAsync(long id, long? vorstellungId, CancellationToken ct);
        Task<SitzreiheEntity> ChangeSitzreiheKategorieAsync(ChangeKategorieSitzreiheDTO dto, CancellationToken ct);
        Task<KinosaalEntity?> DeleteAsync(long id,CancellationToken ct = default);
    }
}
