using KinoAppDB.Entities;
using KinoAppShared.DTOs.Kinosaal;

namespace KinoAppWeb.Services
{
    public interface IKinosaalService
    {
        Task<long> CreateAsync(CreateKinosaalDTO kinosaal, int AnzahlSitzreihen, int GrößeSitzreihen, CancellationToken ct = default);
        Task<KinosaalDTO?> GetKinosaalAsync(long Id, CancellationToken ct);
        Task<SitzreiheEntity> ChangeSitzreiheKategorieAsync(ChangeKategorieSitzreiheDTO dto, CancellationToken ct);
        Task<KinosaalEntity?> DeleteAsync(long id, CancellationToken ct = default);
    }
}
