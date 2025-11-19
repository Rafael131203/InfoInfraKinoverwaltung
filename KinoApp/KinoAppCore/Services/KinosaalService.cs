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

            // Build Sitzreihen + Sitzplaetze in memory
            for (int rowIndex = 0; rowIndex < anzahlSitzreihen; rowIndex++)
            {
                var sitzreihe = new SitzreiheEntity
                {
                    Kategorie = 0,
                    Bezeichnung = $"Reihe {rowIndex + 1}",
                    Sitzplätze = new List<SitzplatzEntity>()
                };

                for (int seatIndex = 0; seatIndex < groesseSitzreihen; seatIndex++)
                {
                    var sitzplatz = new SitzplatzEntity
                    {
                        Gebucht = false,
                        Nummer = seatIndex + 1,
                        Preis = 10m
                    };

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



        public async Task DeleteAsync(KinosaalEntity kinosaal, CancellationToken ct = default)
        {
            await _repoKinosaal.DeleteAsync(kinosaal, ct);
        }
    }
}
