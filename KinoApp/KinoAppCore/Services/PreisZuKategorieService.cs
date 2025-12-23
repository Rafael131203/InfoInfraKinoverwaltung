using KinoAppDB.Entities;
using KinoAppDB.Repository;
using KinoAppShared.DTOs.Kinosaal;
using KinoAppShared.Enums;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace KinoAppCore.Services
{
    public class PreisZuKategorieService : IPreisZuKategorieService
    {

        private readonly IRepository<PreisZuKategorieEntity> _repoPreisZuKategorie;
        private readonly IRepository<SitzplatzEntity> _repoSitzplatz;
        private readonly IRepository<SitzreiheEntity> _repoSitzreihe;

        public PreisZuKategorieService(IRepository<PreisZuKategorieEntity> repoPreisZuKategorie, IRepository<SitzreiheEntity> repoSitzreihe,IRepository<SitzplatzEntity> repoSitzplatz)
        {
            _repoPreisZuKategorie = repoPreisZuKategorie;
            _repoSitzreihe = repoSitzreihe;
            _repoSitzplatz = repoSitzplatz;
        }

        public async Task<PreisZuKategorieEntity?> GetPreisAsync(SitzreihenKategorie kategorie, CancellationToken ct)
        {
            return await _repoPreisZuKategorie
                .Query()
                .FirstOrDefaultAsync(p => p.Kategorie == kategorie, ct);
        }

        public async Task<PreisZuKategorieEntity> SetPreisAsync(SetPreisDTO dto, CancellationToken ct)
        {
            var eintrag = await _repoPreisZuKategorie
                .Query()
                .FirstOrDefaultAsync(p => p.Kategorie == dto.Kategorie, ct);

            if (eintrag == null)
            {
                eintrag = new PreisZuKategorieEntity
                {
                    Kategorie = dto.Kategorie,
                    Preis = dto.Preis
                };

                await _repoPreisZuKategorie.AddAsync(eintrag, ct);
            }
            else
            {
                eintrag.Preis = dto.Preis;
                await _repoPreisZuKategorie.UpdateAsync(eintrag, ct);
            }

            await _repoPreisZuKategorie.SaveAsync(ct);

            var sitzreihen = await _repoSitzreihe
                .Query()
                .Where(r => r.Kategorie == dto.Kategorie)
                .ToListAsync(ct);

            if (sitzreihen.Count == 0)
                return eintrag; 

            var reihenIds = sitzreihen.Select(r => r.Id).ToList();

            var sitzplaetze = await _repoSitzplatz
                .Query()
                .Where(s => reihenIds.Contains(s.SitzreiheId))
                .ToListAsync(ct);

            foreach (var platz in sitzplaetze)
            {
                platz.Preis = dto.Preis;
                await _repoSitzplatz.UpdateAsync(platz, ct);
            }

            await _repoSitzplatz.SaveAsync(ct);

            return eintrag;
        }
    }

}
