using KinoAppCore.Services;
using KinoAppDB;
using KinoAppShared.DTOs.Vorstellung;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KinoAppService.Controllers
{
    /// <summary>
    /// API endpoints for managing and querying showings (Vorstellungen).
    /// </summary>
    /// <remarks>
    /// Administrative endpoints allow creating, updating, and deleting showings. Public endpoints provide
    /// read access for day-, hall-, and film-based queries.
    /// </remarks>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class VorstellungController : BaseController
    {
        private readonly IVorstellungService _vorstellungService;

        /// <summary>
        /// Creates a new <see cref="VorstellungController"/>.
        /// </summary>
        /// <param name="vorstellungService">Service used to manage showings.</param>
        /// <param name="scope">Database scope used for transactional execution.</param>
        public VorstellungController(IVorstellungService vorstellungService, IKinoAppDbContextScope scope)
            : base(scope)
        {
            _vorstellungService = vorstellungService;
        }

        /// <summary>
        /// Creates a new showing.
        /// </summary>
        /// <param name="vorstellung">Create request.</param>
        /// <param name="ct">Cancellation token.</param>
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
                    return new BadRequestObjectResult(new { error = ex.Message });
                }
            }, ct);

        /// <summary>
        /// Returns all showings scheduled for the specified day.
        /// </summary>
        /// <param name="datum">Calendar day to query.</param>
        /// <param name="ct">Cancellation token.</param>
        [AllowAnonymous]
        [HttpGet("VonTag")]
        public Task<IActionResult> GetVorstellungenVonTag(DateTime datum, CancellationToken ct) =>
            ExecuteAsync(async token =>
            {
                var vorstellungen = await _vorstellungService.GetVorstellungVonTagAsync(datum, token);
                return new OkObjectResult(vorstellungen);
            }, ct);

        /// <summary>
        /// Returns all showings scheduled in the specified auditorium.
        /// </summary>
        /// <param name="kinosaalId">Auditorium identifier.</param>
        /// <param name="ct">Cancellation token.</param>
        [AllowAnonymous]
        [HttpGet("VonKinosaal")]
        public Task<IActionResult> GetVorstellungVonKinosaal(long kinosaalId, CancellationToken ct) =>
            ExecuteAsync(async token =>
            {
                var vorstellungen = await _vorstellungService.GetVorstellungVonKinosaalAsync(kinosaalId, token);

                if (!vorstellungen.Any())
                    return new NotFoundObjectResult("Keine Vorstellungen gefunden.");

                return new OkObjectResult(vorstellungen);
            }, ct);

        /// <summary>
        /// Returns all showings for the specified auditorium on the specified day.
        /// </summary>
        /// <param name="datum">Calendar day to query.</param>
        /// <param name="kinosaalId">Auditorium identifier.</param>
        /// <param name="ct">Cancellation token.</param>
        [AllowAnonymous]
        [HttpGet("VonKinosaalUndTag")]
        public Task<IActionResult> GetVorstellungVonKinosaalUndTag(DateTime datum, long kinosaalId, CancellationToken ct) =>
            ExecuteAsync(async token =>
            {
                var vorstellungen = await _vorstellungService.GetVorstellungVonKinosaalUndTagAsync(datum, kinosaalId, token);

                if (!vorstellungen.Any())
                    return new NotFoundObjectResult("Keine Vorstellungen gefunden.");

                return new OkObjectResult(vorstellungen);
            }, ct);

        /// <summary>
        /// Returns all showings scheduled for the specified film.
        /// </summary>
        /// <param name="filmId">Film identifier.</param>
        /// <param name="ct">Cancellation token.</param>
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

        /// <summary>
        /// Updates an existing showing.
        /// </summary>
        /// <param name="dto">Update request.</param>
        /// <param name="ct">Cancellation token.</param>
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

        /// <summary>
        /// Deletes a showing by its identifier.
        /// </summary>
        /// <param name="id">Showing identifier.</param>
        /// <param name="ct">Cancellation token.</param>
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

        /// <summary>
        /// Returns all showings.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        [AllowAnonymous]
        [HttpGet("Alle")]
        public Task<IActionResult> GetAlleVorstellungen(CancellationToken ct) =>
            ExecuteAsync(async token =>
            {
                var vorstellungen = await _vorstellungService.GetAlleVorstellungenAsync(token);
                return new OkObjectResult(vorstellungen);
            }, ct);
    }
}
