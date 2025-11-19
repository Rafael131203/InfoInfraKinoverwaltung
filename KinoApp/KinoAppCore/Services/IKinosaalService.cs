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
        Task CreateAsync(CreateKinosaalDTO kinosaal, int AnzahlSitzreihen, int GrößeSitzreihen, CancellationToken ct = default);
        Task DeleteAsync(KinosaalEntity kinosaal ,CancellationToken ct = default);
    }
}
