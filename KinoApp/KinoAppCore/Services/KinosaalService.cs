using AutoMapper;
using KinoAppDB.Entities;
using KinoAppDB.Repository;
using KinoAppShared.DTOs.Kinosaal;
using KinoAppShared.Enums;
using Microsoft.EntityFrameworkCore;

namespace KinoAppCore.Services
{
    /// <summary>
    /// Default implementation of <see cref="IKinosaalService"/> backed by repositories and EF Core.
    /// </summary>
    /// <remarks>
    /// The creation flow builds the full seating graph (rows + seats) in memory and persists it as a single
    /// aggregate via the auditorium repository.
    /// </remarks>
    public class KinosaalService : IKinosaalService
    {
        private readonly IKinosaalRepository _repoKinosaal;
        private readonly ISitzreiheRepository _repoSitzreihe;
        private readonly ISitzplatzRepository _repoSitzplatz;
        private readonly IPreisZuKategorieService _preisService;
        private readonly ITicketRepository _ticketRepo;
        private readonly IMapper _mapper;

        /// <summary>
        /// Creates a new <see cref="KinosaalService"/>.
        /// </summary>
        public KinosaalService(
            IKinosaalRepository repoKinosaal,
            ISitzreiheRepository repoSitzreihe,
            ISitzplatzRepository repoSitzplatz,
            IPreisZuKategorieService preisService,
            ITicketRepository ticketRepo,
            IMapper mapper)
        {
            _repoKinosaal = repoKinosaal;
            _repoSitzreihe = repoSitzreihe;
            _repoSitzplatz = repoSitzplatz;
            _preisService = preisService;
            _ticketRepo = ticketRepo;
            _mapper = mapper;
        }

        /// <inheritdoc />
        public async Task<long> CreateAsync(CreateKinosaalDTO dto, int anzahlSitzreihen, int groesseSitzreihen, CancellationToken ct = default)
        {
            var kinosaal = _mapper.Map<KinosaalEntity>(dto);
            kinosaal.Sitzreihen = new List<SitzreiheEntity>();

            var countSeat = 1;

            for (int rowIndex = 0; rowIndex < anzahlSitzreihen; rowIndex++)
            {
                var sitzreihe = new SitzreiheEntity
                {
                    Kategorie = SitzreihenKategorie.Parkett,
                    Bezeichnung = $"Reihe {rowIndex + 1}",
                    Sitzplätze = new List<SitzplatzEntity>()
                };

                for (int seatIndex = 0; seatIndex < groesseSitzreihen; seatIndex++)
                {
                    var preisZuKategorie = await _preisService.GetPreisAsync(sitzreihe.Kategorie, ct);
                    if (preisZuKategorie == null)
                    {
                        throw new InvalidOperationException(
                            $"FEHLER: Es wurde kein Preis für die Kategorie '{sitzreihe.Kategorie}' gefunden! " +
                            "Bitte erstelle zuerst Einträge in der Tabelle 'PreisZuKategorie'.");
                    }

                    var sitzplatz = new SitzplatzEntity
                    {
                        Nummer = countSeat,
                        Preis = preisZuKategorie.Preis
                    };

                    countSeat++;
                    sitzreihe.Sitzplätze.Add(sitzplatz);
                }

                kinosaal.Sitzreihen.Add(sitzreihe);
            }

            await _repoKinosaal.AddAsync(kinosaal, ct);
            await _repoKinosaal.SaveAsync(ct);
            return kinosaal.Id;
        }

        /// <inheritdoc />
        public async Task<KinosaalDTO?> GetKinosaalAsync(long id, long? vorstellungId, CancellationToken ct)
        {
            var kinosaal = await _repoKinosaal.Query()
                .Where(k => k.Id == id)
                .Include(k => k.Sitzreihen)
                    .ThenInclude(r => r.Sitzplätze)
                .FirstOrDefaultAsync(ct);

            if (kinosaal == null)
                return null;

            var ticketsBySeat = new Dictionary<long, TicketEntity>();

            if (vorstellungId.HasValue)
            {
                var tickets = await _ticketRepo.Query()
                    .Where(t => t.VorstellungId == vorstellungId.Value)
                    .ToListAsync(ct);

                ticketsBySeat = tickets.ToDictionary(t => t.SitzplatzId);
            }

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
                        Kategorie = r.Kategorie,
                        Sitzplätze = r.Sitzplätze
                            .OrderBy(s => s.Nummer)
                            .Select(s =>
                            {
                                ticketsBySeat.TryGetValue(s.Id, out var ticket);

                                var status = ticket == null
                                    ? TicketStatus.Free
                                    : (TicketStatus)ticket.Status;

                                return new SitzplatzDTO
                                {
                                    Id = s.Id,
                                    Nummer = s.Nummer,
                                    Preis = s.Preis,
                                    Status = status,
                                    Gebucht = status == TicketStatus.Reserved || status == TicketStatus.Booked
                                };
                            })
                            .ToList()
                    })
                    .ToList()
            };

            return dto;
        }

        /// <inheritdoc />
        public async Task<SitzreiheEntity?> ChangeSitzreiheKategorieAsync(ChangeKategorieSitzreiheDTO dto, CancellationToken ct)
        {
            var sitzreihe = await _repoSitzreihe.GetByIdAsync(dto.Id, ct);
            if (sitzreihe == null)
                return null;

            if (!Enum.IsDefined(typeof(SitzreihenKategorie), dto.Kategorie))
                throw new ArgumentException("Ungültige Kategorie");

            var sitzplaetze = await _repoSitzplatz
                .Query()
                .Where(s => s.SitzreiheId == sitzreihe.Id)
                .ToListAsync(ct);

            sitzreihe.Sitzplätze = sitzplaetze;

            sitzreihe.Kategorie = dto.Kategorie;

            var preisZuKategorie = await _preisService.GetPreisAsync(dto.Kategorie, ct);
            foreach (var platz in sitzreihe.Sitzplätze)
            {
                platz.Preis = preisZuKategorie.Preis;
            }

            await _repoSitzreihe.UpdateAsync(sitzreihe, ct);
            await _repoSitzreihe.SaveAsync(ct);

            return sitzreihe;
        }

        /// <inheritdoc />
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
