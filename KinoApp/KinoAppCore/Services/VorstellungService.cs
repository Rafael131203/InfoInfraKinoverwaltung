using AutoMapper;
using KinoAppDB.Entities;
using KinoAppDB.Repository;
using KinoAppShared.DTOs.Vorstellung;
using Microsoft.EntityFrameworkCore;

namespace KinoAppCore.Services
{
    public class VorstellungService : IVorstellungService
    {
        private readonly IVorstellungRepository _repoVorstellung;
        private readonly IFilmRepository _repoFilm;
        private readonly IMapper _mapper;
        private readonly ITicketService _ticketService;

        public VorstellungService(
            IVorstellungRepository repoVorstellung,
            IFilmRepository repoFilm,
            IMapper mapper,
            ITicketService ticketService)
        {
            _repoVorstellung = repoVorstellung;
            _repoFilm = repoFilm;
            _mapper = mapper;
            _ticketService = ticketService;
        }

        public async Task CreateVorstellungAsync(CreateVorstellungDTO vorstellungDto, CancellationToken ct)
        {
            var startZeit = vorstellungDto.Datum;

            var film = await _repoFilm.GetByIdAsync(vorstellungDto.FilmId, ct);
            if (film == null)
                throw new ArgumentException($"Film mit Id {vorstellungDto.FilmId} existiert nicht.");

            var endZeit = startZeit.AddMinutes(GetRuntimeMinutes(film.Dauer));

            var existingVorstellungen = (await _repoVorstellung.GetAllAsync(ct))
                .Where(v => v.KinosaalId == vorstellungDto.KinosaalId);

            foreach (var v in existingVorstellungen)
            {
                var vFilm = await _repoFilm.GetByIdAsync(v.FilmId, ct);
                if (vFilm == null) continue;

                var vStart = v.Datum;
                var vEnd = vStart.AddMinutes(GetRuntimeMinutes(vFilm.Dauer));

                if (startZeit < vEnd && endZeit > vStart)
                    throw new InvalidOperationException("Die Vorstellung überschneidet sich mit einer bestehenden Vorstellung im selben Kinosaal.");
            }

            var entity = new VorstellungEntity
            {
                Datum = vorstellungDto.Datum,
                FilmId = vorstellungDto.FilmId,
                KinosaalId = vorstellungDto.KinosaalId,
                Status = vorstellungDto.Status
            };

            await _repoVorstellung.AddAsync(entity, ct);
            await _repoVorstellung.SaveAsync(ct);

            await _ticketService.CreateTicketsForVorstellungAsync(entity.Id, entity.KinosaalId, ct);
        }


        private static int GetRuntimeMinutes(int? dauer)
        {
            if (!dauer.HasValue)
                return 120; // default 2h

            var d = dauer.Value;

            // If it looks like seconds (e.g. 5400), convert to minutes.
            if (d > 300)         // > 5h is unrealistic in minutes, so treat as seconds
                return d / 60;

            return d;            // already minutes
        }



        private IQueryable<VorstellungEntity> BaseQuery()
        {
            return _repoVorstellung
                .Query()
                .Include(v => v.Film)
                .Include(v => v.Kinosaal)
                    .ThenInclude(k => k.Sitzreihen)
                        .ThenInclude(r => r.Sitzplätze);
        }

        public async Task<List<VorstellungDTO>> GetVorstellungVonTagAsync(DateTime datum, CancellationToken ct)
        {
            var start = datum.Date;
            var end = start.AddDays(1);

            var data = await BaseQuery()
                .Where(v => v.Datum >= start && v.Datum < end)
                .OrderBy(v => v.Datum)
                .ToListAsync(ct);

            return _mapper.Map<List<VorstellungDTO>>(data);
        }

        public async Task<List<VorstellungDTO>> GetVorstellungVonKinosaalAsync(long kinosaalId, CancellationToken ct)
        {
            var data = await BaseQuery()
                .Where(v => v.KinosaalId == kinosaalId)
                .OrderBy(v => v.Datum)
                .ToListAsync(ct);

            return _mapper.Map<List<VorstellungDTO>>(data);
        }

        public async Task<List<VorstellungDTO>> GetVorstellungVonKinosaalUndTagAsync(DateTime datum, long kinosaalId, CancellationToken ct)
        {
            var start = datum.Date;
            var end = start.AddDays(1);

            var data = await BaseQuery()
                .Where(v => v.KinosaalId == kinosaalId && v.Datum >= start && v.Datum < end)
                .OrderBy(v => v.Datum)
                .ToListAsync(ct);

            return _mapper.Map<List<VorstellungDTO>>(data);
        }

        public async Task<List<VorstellungDTO>> GetVorstellungVonFilm(string filmId, CancellationToken ct)
        {
            var data = await BaseQuery()
                .Where(v => v.FilmId == filmId)
                .OrderBy(v => v.Datum)
                .ToListAsync(ct);

            return _mapper.Map<List<VorstellungDTO>>(data);
        }

        public async Task<UpdateVorstellungResultDTO?> UpdateVorstellungAsync(UpdateVorstellungDTO dto,CancellationToken ct)
        {
            var vorstellung = await _repoVorstellung
                .Query(false)
                .Include(v => v.Film)
                .Include(v => v.Kinosaal)
                    .ThenInclude(k => k.Sitzreihen)
                        .ThenInclude(r => r.Sitzplätze)
                .FirstOrDefaultAsync(v => v.Id == dto.Id, ct);

            if (vorstellung == null)
                throw new ArgumentException($"Vorstellung mit Id {dto.Id} existiert nicht.");

            // ALTE VERSION MAPPEN
            var alteVersion = _mapper.Map<VorstellungDTO>(vorstellung);


            var neuesDatum = dto.Datum ?? vorstellung.Datum;

            var neuerFilmId =
                    string.IsNullOrWhiteSpace(dto.FilmId) || dto.FilmId == "string"
                    ? vorstellung.FilmId
                    : dto.FilmId;

            // Film laden
            var film = await _repoFilm.GetByIdAsync(neuerFilmId, ct);
            if (film == null)
                throw new ArgumentException($"Film mit Id {neuerFilmId} existiert nicht.");

            // Endzeit berechnen
            var endZeit = neuesDatum.AddSeconds(film.Dauer ?? 0);

            // Überschneidungsprüfung
            var existing = (await _repoVorstellung.GetAllAsync(ct))
                .Where(v => v.KinosaalId == vorstellung.KinosaalId && v.Id != dto.Id);

            foreach (var v in existing)
            {
                var vFilm = await _repoFilm.GetByIdAsync(v.FilmId, ct);
                if (vFilm == null) continue;

                var vStart = v.Datum;
                var vEnd = vStart.AddSeconds(vFilm.Dauer ?? 0);

                if (neuesDatum < vEnd && endZeit > vStart)
                    throw new InvalidOperationException("Die aktualisierte Vorstellung überschneidet sich mit einer anderen Vorstellung.");
            }

            // ÄNDERUNGEN ÜBERNEHMEN
            vorstellung.Datum = neuesDatum;
            vorstellung.FilmId = neuerFilmId;

            await _repoVorstellung.UpdateAsync(vorstellung, ct);
            await _repoVorstellung.SaveAsync(ct);

            // NEUE VERSION MAPPEN (mit geladenem Film/Kinosaal)
            var neueVersion = _mapper.Map<VorstellungDTO>(vorstellung);

            return new UpdateVorstellungResultDTO
            {
                VorstellungAlt = alteVersion,
                VorstellungNeu = neueVersion
            };
        }


        public async Task<bool> DeleteVorstellungAsync(long id, CancellationToken ct)
        {
            var vorstellung = await _repoVorstellung.GetByIdAsync(id, ct);
            if (vorstellung == null)
                return false;

            await _repoVorstellung.DeleteAsync(vorstellung, ct);
            await _repoVorstellung.SaveAsync(ct);
            return true;
        }
    }
}
