using AutoMapper;
using KinoAppCore.Abstractions;
using KinoAppDB.Entities;
using KinoAppDB.Repository;
using KinoAppShared.DTOs.Authentication;
using KinoAppShared.DTOs.Kinosaal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinoAppCore.Services
{
    public class KinosaalService : IKinosaalService
    {
        private readonly IKinosaalRepository _repoKinosaal;
        private readonly ISitzreiheRepository _repoSitzreihe;
        private readonly ISitzplatzRepository _repoSitzplatz;
        private readonly IMapper _mapper;

        public KinosaalService(IKinosaalRepository repoKinosaal, ISitzreiheRepository repoSitzreihe, ISitzplatzRepository repoSitzplatz, IMapper mapper)
        {
            _repoKinosaal = repoKinosaal;
            _repoSitzreihe = repoSitzreihe;
            _repoSitzplatz = repoSitzplatz;
            _mapper = mapper;
        }

        public async Task CreateAsync(CreateKinosaalDTO dto, int anzahlSitzreihen, int groesseSitzreihen, CancellationToken ct = default)
        {
            // Map basic Kinosaal data (Name)
            var kinosaal = _mapper.Map<KinosaalEntity>(dto);
            kinosaal.Sitzreihen = new List<SitzreiheEntity>();
            var countSeat = 1;
            // Build Sitzreihen + Sitzplaetze in memory
            for (int rowIndex = 0; rowIndex < anzahlSitzreihen; rowIndex++)
            {
                
                var sitzreihe = new SitzreiheEntity
                {
                    Kategorie = SitzreihenKategorie.LOGE,
                    Bezeichnung = $"Reihe {rowIndex + 1}",
                    Sitzplätze = new List<SitzplatzEntity>()
                };

                for (int seatIndex = 0; seatIndex < groesseSitzreihen; seatIndex++)
                {
                    
                    var sitzplatz = new SitzplatzEntity
                    {
                        Gebucht = false,
                        Nummer = countSeat,
                        Preis = 10m
                    };
                    countSeat++;
                    // Attach seat to row (navigation only, no FK set)
                    sitzreihe.Sitzplätze.Add(sitzplatz);
                }

                // Attach row to kinosaal (navigation only)
                kinosaal.Sitzreihen.Add(sitzreihe);
            }

            // Only add the root entity; EF will discover and insert the whole graph
            await _repoKinosaal.AddAsync(kinosaal, ct);
            await _repoKinosaal.SaveAsync(ct);
        }

        public async Task<KinosaalEntity?> GetKinosaalAsync(long id, CancellationToken ct)
        {
            var kinosaal = await _repoKinosaal.GetByIdAsync(id, ct);
            if (kinosaal == null) return null;
            return kinosaal;
            
        }

        public async Task<SitzreiheEntity?> ChangeSitzreiheKategorieAsync(ChangeKategorieSitzreiheDTO dto, CancellationToken ct)
        {
            // 1. Sitzreihe laden
            var sitzreihe = await _repoSitzreihe.GetByIdAsync(dto.Id, ct);
            if (sitzreihe == null)
                return null;

            if (!Enum.IsDefined(typeof(SitzreihenKategorie), dto.Kategorie))
                throw new ArgumentException("Ungültige Kategorie");

            // 2. Kategorie ändern
            sitzreihe.Kategorie = dto.Kategorie;

            // 3. Speichern
            await _repoSitzreihe.UpdateAsync(sitzreihe);
            await _repoSitzreihe.SaveAsync();

            // 4. Ergebnis zurückgeben
            return sitzreihe;
        }

        public async Task<KinosaalEntity?> DeleteAsync(long id, CancellationToken ct = default)
        {
            var kinosaal = await _repoKinosaal.GetByIdAsync(id, ct);
            if (kinosaal == null)
                return null;

            await _repoKinosaal.DeleteAsync(kinosaal);
            await _repoKinosaal.SaveAsync();
            return kinosaal;
        }
    }
}
