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
        Task <List<VorstellungEntity>> GetVorstellungVonTagAsync (DateTime datum, CancellationToken ct);
        Task<bool> DeleteVorstellungAsync(long id, CancellationToken ct);
    }
}
