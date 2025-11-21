using AutoMapper;
using KinoAppDB.Entities;
using KinoAppDB.Repository;
using KinoAppShared.DTOs.Kinosaal;
using KinoAppShared.DTOs.Vorstellung;
using KinoAppShared.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinoAppCore.Services
{
    public class VorstellungService : IVorstellungService
    {
        private readonly IVorstellungRepository _repoVorstellung;
        private readonly IFilmRepository _repoFilm;
        private readonly IMapper _mapper;

        public VorstellungService(IVorstellungRepository repoVorstellung, IFilmRepository repoFilm, IMapper mapper)
        {
            _repoVorstellung = repoVorstellung;
            _repoFilm = repoFilm;
            _mapper = mapper;
        }

        public async Task CreateVorstellungAsync(CreateVorstellungDTO vorstellungDto, CancellationToken ct)
        {
            // Berechne Start- und Endzeit der neuen Vorstellung
            var startZeit = vorstellungDto.Datum;

            var film = await _repoFilm.GetByIdAsync(vorstellungDto.FilmId, ct);
            if (film == null)
                throw new ArgumentException($"Film mit Id {vorstellungDto.FilmId} existiert nicht.");

            // Dauer in Sekunden -> Endzeit berechnen
            var endZeit = startZeit.AddSeconds(film.Dauer ?? 0);

            // Alle Vorstellungen im gleichen Kinosaal laden
            var existingVorstellungen = (await _repoVorstellung.GetAllAsync(ct))
                .Where(v => v.KinosaalId == vorstellungDto.KinosaalId);

            // Überschneidung prüfen
            foreach (var v in existingVorstellungen)
            {
                var vFilm = await _repoFilm.GetByIdAsync(v.FilmId, ct);
                if (vFilm == null) continue;

                var vStart = v.Datum;
                var vEnd = vStart.AddSeconds(vFilm.Dauer ?? 0);

                bool overlaps = startZeit < vEnd && endZeit > vStart;
                if (overlaps)
                    throw new InvalidOperationException("Die Vorstellung überschneidet sich mit einer bestehenden Vorstellung im selben Kinosaal.");
            }

            // Keine Überschneidung -> Vorstellung erstellen
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

        public async Task<List<VorstellungEntity>> GetVorstellungVonTagAsync(DateTime datum, CancellationToken ct = default)
        {
            var start = datum.Date;
            var end = start.AddDays(1);

            return await _repoVorstellung
                .Query()
                .Where(v => v.Datum >= start && v.Datum < end)
                .ToListAsync(ct);
        }

        public async Task<bool> DeleteVorstellungAsync(long id, CancellationToken ct)
        {
            // Hole die Vorstellung aus der Datenbank
            var vorstellung = await _repoVorstellung.GetByIdAsync(id, ct);
            if (vorstellung == null)
                return false;

            // Lösche die Vorstellung
            await _repoVorstellung.DeleteAsync(vorstellung, ct);
            await _repoVorstellung.SaveAsync(ct);
            return true;
        }


    }
}
