using KinoAppDB.Entities;
using KinoAppShared.DTOs.Authentication;
using KinoAppShared.DTOs.Kinosaal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinoAppCore.Services
{
    public interface IKinosaalService
    {
        Task<long> CreateAsync(CreateKinosaalDTO kinosaal, int AnzahlSitzreihen, int GrößeSitzreihen, CancellationToken ct = default);
        Task<KinosaalDTO?> GetKinosaalAsync(long Id, CancellationToken ct);
        Task<SitzreiheEntity> ChangeSitzreiheKategorieAsync(ChangeKategorieSitzreiheDTO dto, CancellationToken ct);
        Task<KinosaalEntity?> DeleteAsync(long id,CancellationToken ct = default);
    }
}
