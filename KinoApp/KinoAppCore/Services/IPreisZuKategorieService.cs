using KinoAppDB.Entities;
using KinoAppShared.DTOs.Kinosaal;
using KinoAppShared.Enums;

namespace KinoAppCore.Services
{
    public interface IPreisZuKategorieService
    {
        Task<PreisZuKategorieEntity?> GetPreisAsync(SitzreihenKategorie kategorie, CancellationToken ct);
        Task<PreisZuKategorieEntity> SetPreisAsync(SetPreisDTO setPreisDTO, CancellationToken ct);
    }

}
