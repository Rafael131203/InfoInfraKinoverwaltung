using KinoAppDB.Entities;
using KinoAppShared.DTOs.Vorstellung;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinoAppCore.Services
{
    public interface IVorstellungService
    {
        Task CreateVorstellungAsync (CreateVorstellungDTO vorstellung, CancellationToken ct);
        Task<UpdateVorstellungResultDTO?> UpdateVorstellungAsync (UpdateVorstellungDTO vorstellung, CancellationToken ct);
        Task <List<VorstellungDTO>> GetVorstellungVonTagAsync (DateTime datum, CancellationToken ct);
        Task <List<VorstellungDTO>> GetVorstellungVonKinosaalAsync (long KinosaalId, CancellationToken ct);
        Task <List<VorstellungDTO>> GetVorstellungVonKinosaalUndTagAsync (DateTime datum, long KinosaalId, CancellationToken ct);
        Task<List<VorstellungDTO>>  GetVorstellungVonFilm(string filmId, CancellationToken ct);
        Task<bool> DeleteVorstellungAsync(long id, CancellationToken ct);
    }
}
