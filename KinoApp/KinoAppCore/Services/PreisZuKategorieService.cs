using KinoAppDB.Entities;
using KinoAppDB.Repository;
using KinoAppShared.DTOs.Kinosaal;
using KinoAppShared.Enums;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using KinoAppShared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static KinoAppCore.Services.PreisZuKategorieService;

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
            // 1. Preis Eintrag lesen oder neu anlegen
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

            // 2. Alle Sitzreihen dieser Kategorie holen
            var sitzreihen = await _repoSitzreihe
                .Query()
                .Where(r => r.Kategorie == dto.Kategorie)
                .ToListAsync(ct);

            if (sitzreihen.Count == 0)
                return eintrag; // nichts zu aktualisieren

            // 3. IDs der Sitzreihen extrahieren
            var reihenIds = sitzreihen.Select(r => r.Id).ToList();

            // 4. Alle Sitzplätze der passenden Sitzreihen laden
            var sitzplaetze = await _repoSitzplatz
                .Query()
                .Where(s => reihenIds.Contains(s.SitzreiheId))
                .ToListAsync(ct);

            // 5. Preis aktualisieren
            foreach (var platz in sitzplaetze)
            {
                platz.Preis = dto.Preis;
                await _repoSitzplatz.UpdateAsync(platz, ct);
            }

            // 6. Speichern
            await _repoSitzplatz.SaveAsync(ct);

            return eintrag;
        }
    }

}
