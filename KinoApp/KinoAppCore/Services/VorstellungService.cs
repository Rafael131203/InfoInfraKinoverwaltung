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

        public VorstellungService(
            IVorstellungRepository repoVorstellung,
            IFilmRepository repoFilm,
            IMapper mapper)
        {
            _repoVorstellung = repoVorstellung;
            _repoFilm = repoFilm;
            _mapper = mapper;
        }

        public async Task CreateVorstellungAsync(CreateVorstellungDTO vorstellungDto, CancellationToken ct)
        {
            var startZeit = vorstellungDto.Datum;

            var film = await _repoFilm.GetByIdAsync(vorstellungDto.FilmId, ct);
            if (film == null)
                throw new ArgumentException($"Film mit Id {vorstellungDto.FilmId} existiert nicht.");

            var endZeit = startZeit.AddSeconds(film.Dauer ?? 0);

            var existingVorstellungen = (await _repoVorstellung.GetAllAsync(ct))
                .Where(v => v.KinosaalId == vorstellungDto.KinosaalId);

            foreach (var v in existingVorstellungen)
            {
                var vFilm = await _repoFilm.GetByIdAsync(v.FilmId, ct);
                if (vFilm == null) continue;

                var vStart = v.Datum;
                var vEnd = vStart.AddSeconds(vFilm.Dauer ?? 0);

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
