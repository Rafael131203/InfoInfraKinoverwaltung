using KinoAppDB.Entities;
using KinoAppDB.Repository;
using KinoAppShared.DTOs.Kinosaal;
using KinoAppShared.Enums;
using Microsoft.EntityFrameworkCore;

namespace KinoAppCore.Services
{
    /// <summary>
    /// Manages pricing for seat row categories and propagates changes to affected seats.
    /// </summary>
    /// <remarks>
    /// A category price is stored in <see cref="PreisZuKategorieEntity"/> and is applied to all seats in rows
    /// of the same <see cref="SitzreihenKategorie"/>. Updating a category price updates both the category entry
    /// and the prices of all seats currently assigned to that category.
    /// </remarks>
    public class PreisZuKategorieService : IPreisZuKategorieService
    {
        private readonly IRepository<PreisZuKategorieEntity> _repoPreisZuKategorie;
        private readonly IRepository<SitzplatzEntity> _repoSitzplatz;
        private readonly IRepository<SitzreiheEntity> _repoSitzreihe;

        /// <summary>
        /// Creates a new <see cref="PreisZuKategorieService"/>.
        /// </summary>
        public PreisZuKategorieService(
            IRepository<PreisZuKategorieEntity> repoPreisZuKategorie,
            IRepository<SitzreiheEntity> repoSitzreihe,
            IRepository<SitzplatzEntity> repoSitzplatz)
        {
            _repoPreisZuKategorie = repoPreisZuKategorie;
            _repoSitzreihe = repoSitzreihe;
            _repoSitzplatz = repoSitzplatz;
        }

        /// <inheritdoc />
        public async Task<PreisZuKategorieEntity?> GetPreisAsync(SitzreihenKategorie kategorie, CancellationToken ct)
        {
            return await _repoPreisZuKategorie
                .Query()
                .FirstOrDefaultAsync(p => p.Kategorie == kategorie, ct);
        }

        /// <inheritdoc />
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
