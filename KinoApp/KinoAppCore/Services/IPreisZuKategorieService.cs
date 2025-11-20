using KinoAppDB.Entities;
using KinoAppShared.DTOs.Kinosaal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinoAppCore.Services
{
    public interface IPreisZuKategorieService
    {
        Task<PreisZuKategorieEntity?> GetPreisAsync(SitzreihenKategorie kategorie, CancellationToken ct);
        Task<PreisZuKategorieEntity> SetPreisAsync(SetPreisDTO setPreisDTO, CancellationToken ct);
    }

}
