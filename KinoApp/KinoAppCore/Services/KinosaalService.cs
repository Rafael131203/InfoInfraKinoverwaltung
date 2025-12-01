using AutoMapper;
using KinoAppCore.Abstractions;
using KinoAppCore.Entities;
using KinoAppDB.Entities;
using KinoAppDB.Repository;
using KinoAppShared.DTOs.Authentication;
using KinoAppShared.DTOs.Kinosaal;
using KinoAppShared.Enums;
using Microsoft.EntityFrameworkCore;
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
        private readonly IPreisZuKategorieService _preisService;
        private readonly ITicketRepository _ticketRepo;
        private readonly IMapper _mapper;

        public KinosaalService(IKinosaalRepository repoKinosaal, ISitzreiheRepository repoSitzreihe, ISitzplatzRepository repoSitzplatz, IPreisZuKategorieService preisService, ITicketRepository ticketRepo, IMapper mapper)
        {
            _repoKinosaal = repoKinosaal;
            _repoSitzreihe = repoSitzreihe;
            _repoSitzplatz = repoSitzplatz;
            _preisService = preisService;
            _ticketRepo = ticketRepo;
            _mapper = mapper;
        }

        public async Task<long> CreateAsync(CreateKinosaalDTO dto, int anzahlSitzreihen, int groesseSitzreihen, CancellationToken ct = default)
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
                    Kategorie = SitzreihenKategorie.Parkett, // Standard: Parkett
                    Bezeichnung = $"Reihe {rowIndex + 1}",
                    Sitzplätze = new List<SitzplatzEntity>()
                };

                for (int seatIndex = 0; seatIndex < groesseSitzreihen; seatIndex++)
                {
                    // 1. Preis laden
                    var preisZuKategorie = await _preisService.GetPreisAsync(sitzreihe.Kategorie, ct);

                    // 2. Safety Check (Hier ist dein neuer Code!)
                    if (preisZuKategorie == null)
                    {
                        throw new InvalidOperationException(
                            $"FEHLER: Es wurde kein Preis für die Kategorie '{sitzreihe.Kategorie}' gefunden! " +
                            "Bitte erstelle zuerst Einträge in der Tabelle 'PreisZuKategorie'.");
                    }

                    // 3. Sitzplatz erstellen (Jetzt sicher, da nicht null)
                    var sitzplatz = new SitzplatzEntity
                    {
                        Nummer = countSeat,
                        Preis = preisZuKategorie.Preis // Sicherer Zugriff
                    };

                    countSeat++;
                    sitzreihe.Sitzplätze.Add(sitzplatz);
                }

                kinosaal.Sitzreihen.Add(sitzreihe);
            }

            // Only add the root entity; EF will discover and insert the whole graph
            await _repoKinosaal.AddAsync(kinosaal, ct);
            await _repoKinosaal.SaveAsync(ct);
            return kinosaal.Id;
        }

        public async Task<KinosaalDTO?> GetKinosaalAsync(long id, long? vorstellungId, CancellationToken ct)
        {
            // 1. Load hall with rows + seats
            var kinosaal = await _repoKinosaal.Query()
                .Where(k => k.Id == id)
                .Include(k => k.Sitzreihen)
                    .ThenInclude(r => r.Sitzplätze)
                .FirstOrDefaultAsync(ct);

            if (kinosaal == null)
                return null;

            // 2. Load tickets for this Vorstellung (if given)
            var ticketsBySeat = new Dictionary<long, TicketEntity>();

            if (vorstellungId.HasValue)
            {
                var tickets = await _ticketRepo.Query()
                    .Where(t => t.VorstellungId == vorstellungId.Value)
                    .ToListAsync(ct);

                ticketsBySeat = tickets.ToDictionary(t => t.SitzplatzId);
            }

            // 3. Manually map to DTO so Sitzplätze is ALWAYS filled
            var dto = new KinosaalDTO
            {
                Id = kinosaal.Id,
                Name = kinosaal.Name,
                Sitzreihen = kinosaal.Sitzreihen
                    .OrderBy(r => r.Bezeichnung)
                    .Select(r => new SitzreiheDTO
                    {
                        Id = r.Id,
                        Bezeichnung = r.Bezeichnung,
                        Kategorie = r.Kategorie, // already SitzreihenKategorie enum
                        Sitzplätze = r.Sitzplätze
                            .OrderBy(s => s.Nummer)
                            .Select(s =>
                            {
                                ticketsBySeat.TryGetValue(s.Id, out var ticket);

                                // If no ticket exists for this seat+show, seat is free
                                var status = ticket == null
                                    ? TicketStatus.Free
                                    : (TicketStatus)ticket.Status;

                                return new SitzplatzDTO
                                {
                                    Id = s.Id,
                                    Nummer = s.Nummer,
                                    Preis = s.Preis,
                                    Status = status,
                                    // reserved OR booked = taken for everyone else
                                    Gebucht = status == TicketStatus.Reserved || status == TicketStatus.Booked
                                };
                            })
                            .ToList()
                    })
                    .ToList()
            };

            return dto;
        }


        public async Task<SitzreiheEntity?> ChangeSitzreiheKategorieAsync(ChangeKategorieSitzreiheDTO dto, CancellationToken ct)
        {
            // 1. Sitzreihe laden
            var sitzreihe = await _repoSitzreihe.GetByIdAsync(dto.Id, ct);
            if (sitzreihe == null)
                return null;

            if (!Enum.IsDefined(typeof(SitzreihenKategorie), dto.Kategorie))
                throw new ArgumentException("Ungültige Kategorie");

            // 2. Alle Sitzplätze der Sitzreihe laden
            var sitzplaetze = await _repoSitzplatz
                .Query()
                .Where(s => s.SitzreiheId == sitzreihe.Id)
                .ToListAsync(ct);

            sitzreihe.Sitzplätze = sitzplaetze;

            // 3. Kategorie ändern
            sitzreihe.Kategorie = dto.Kategorie;

            // 4. Preis der Sitzplätze aktualisieren
            var preisZuKategorie = await _preisService.GetPreisAsync(dto.Kategorie, ct);
            foreach (var platz in sitzreihe.Sitzplätze)
            {
                platz.Preis = preisZuKategorie.Preis;
            }


            // 5. Sitzreihe speichern
            await _repoSitzreihe.UpdateAsync(sitzreihe, ct);
            await _repoSitzreihe.SaveAsync(ct);

            // 6. Ergebnis zurückgeben
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
