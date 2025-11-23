using KinoAppCore.Services;
using KinoAppDB;
using KinoAppDB.Entities;
using KinoAppShared.DTOs.Kinosaal;
using KinoAppShared.DTOs.Vorstellung;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KinoAppService.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class VorstellungController : BaseController
    {
        private readonly IVorstellungService _vorstellungService;

        public VorstellungController(IVorstellungService vorstellungService, IKinoAppDbContextScope scope)
            : base(scope)
        {
            _vorstellungService = vorstellungService;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("Erstellen")]
        public Task<IActionResult> VorstellungErstellen(CreateVorstellungDTO vorstellung, CancellationToken ct) =>
            ExecuteAsync(async token =>
            {
                try
                {
                    await _vorstellungService.CreateVorstellungAsync(vorstellung, token);
                    return new OkResult();
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("überschneidet"))
                {
                    // Überschneidung -> 400 Bad Request
                    return new BadRequestObjectResult(new { error = ex.Message });
                }
            }, ct);

        [AllowAnonymous]
        [HttpGet("VonTag")]
        public Task<IActionResult> GetVorstellungenVonTag(DateTime datum, CancellationToken ct) =>
            ExecuteAsync(async token =>
            {
                var vorstellungen = await _vorstellungService.GetVorstellungVonTagAsync(datum, token);

                //if (!vorstellungen.Any())
                //    return new NotFoundObjectResult($"Keine Vorstellungen am {datum:yyyy-MM-dd} gefunden.");

                return new OkObjectResult(vorstellungen);
            }, ct);

        [AllowAnonymous]
        [HttpGet("VonKinosaal")]
        public Task<IActionResult> GetVorstellungVonKinosaal(long kinosaalId, CancellationToken ct) =>
            ExecuteAsync(async token =>
            {
                var vorstellungen = await _vorstellungService.GetVorstellungVonKinosaalAsync(kinosaalId, token);

                if (!vorstellungen.Any())
                    return new NotFoundObjectResult($"Keine Vorstellungen gefunden.");

                return new OkObjectResult(vorstellungen);
            }, ct);

        [AllowAnonymous]
        [HttpGet("VonKinosaalUndTag")]
        public Task<IActionResult> GetVorstellungVonKinosaalUndTag(DateTime datum, long kinosaalId, CancellationToken ct) =>
            ExecuteAsync(async token =>
            {
                var vorstellungen = await _vorstellungService.GetVorstellungVonKinosaalUndTagAsync(datum, kinosaalId, token);

                if (!vorstellungen.Any())
                    return new NotFoundObjectResult($"Keine Vorstellungen gefunden.");

                return new OkObjectResult(vorstellungen);
            }, ct);

        [AllowAnonymous]
        [HttpGet("VonFilm")]
        public Task<IActionResult> GetVorstellungenVonFilm(string filmId, CancellationToken ct) =>
            ExecuteAsync(async token =>
            {
                var vorstellungen = await _vorstellungService.GetVorstellungVonFilm(filmId, token);

                if (!vorstellungen.Any())
                    return new NotFoundObjectResult($"Keine Vorstellungen für FilmId {filmId} gefunden.");

                return new OkObjectResult(vorstellungen);
            }, ct);

        [Authorize(Roles = "Admin")]
        [HttpPut]
        public Task<IActionResult> VorstellungAktualisieren(UpdateVorstellungDTO dto, CancellationToken ct) =>
            ExecuteAsync(async token =>
            {
                var result = await _vorstellungService.UpdateVorstellungAsync(dto, token);

                if (result == null)
                    return new NotFoundObjectResult($"Vorstellung mit Id {dto.Id} nicht gefunden.");

                return new OkObjectResult(result);
            }, ct);

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:long}")]
        public Task<IActionResult> VorstellungLoeschen(long id, CancellationToken ct) =>
            ExecuteAsync(async token =>
            {
                var deleted = await _vorstellungService.DeleteVorstellungAsync(id, token);

                if (!deleted)
                    return new NotFoundObjectResult($"Vorstellung mit Id {id} nicht gefunden.");

                return new OkResult();
            }, ct);
    }
}
