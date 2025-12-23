using KinoAppCore.Services;
using KinoAppDB;
using KinoAppShared.DTOs.Authentication;
using KinoAppShared.DTOs.Kinosaal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KinoAppService.Controllers
{
    /// <summary>
    /// API endpoints for auditorium (Kinosaal) management and seat pricing configuration.
    /// </summary>
    /// <remarks>
    /// Most endpoints require authentication. Administrative operations additionally require the <c>Admin</c> role.
    /// </remarks>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class KinosaalController : BaseController
    {
        private readonly IKinosaalService _kinosaalService;
        private readonly IPreisZuKategorieService _preisService;

        /// <summary>
        /// Creates a new <see cref="KinosaalController"/>.
        /// </summary>
        /// <param name="kinosaalService">Service used to manage auditoriums and seating.</param>
        /// <param name="preisService">Service used to configure category prices.</param>
        /// <param name="scope">Database scope used for transactional execution.</param>
        public KinosaalController(
            IKinosaalService kinosaalService,
            IPreisZuKategorieService preisService,
            IKinoAppDbContextScope scope)
            : base(scope)
        {
            _kinosaalService = kinosaalService;
            _preisService = preisService;
        }

        /// <summary>
        /// Returns an auditorium with its seat rows and seats.
        /// </summary>
        /// <param name="id">Auditorium identifier.</param>
        /// <param name="vorstellungId">
        /// Optional showing identifier used to enrich seat data with current ticket status.
        /// </param>
        /// <param name="ct">Cancellation token.</param>
        [AllowAnonymous]
        [HttpGet]
        public Task<IActionResult> GetKinosaal(long id, long? vorstellungId, CancellationToken ct) =>
            ExecuteAsync(async token =>
            {
                var kinosaal = await _kinosaalService.GetKinosaalAsync(id, vorstellungId, token);
                return Ok(kinosaal);
            }, ct);

        /// <summary>
        /// Creates a new auditorium with a generated seating layout.
        /// </summary>
        /// <param name="dto">Auditorium data.</param>
        /// <param name="AnzahlSitzreihen">Number of seat rows to create.</param>
        /// <param name="GrößeSitzreihen">Number of seats per row.</param>
        /// <param name="ct">Cancellation token.</param>
        [Authorize(Roles = "Admin")]
        [HttpPost("Erstellen")]
        public Task<IActionResult> KinosaalErstellen(CreateKinosaalDTO dto, int AnzahlSitzreihen, int GrößeSitzreihen, CancellationToken ct) =>
            ExecuteAsync(async token =>
            {
                var id = await _kinosaalService.CreateAsync(dto, AnzahlSitzreihen, GrößeSitzreihen, token);
                return new OkObjectResult(new { id });
            }, ct);

        /// <summary>
        /// Changes the category of a seat row and updates seat pricing accordingly.
        /// </summary>
        /// <param name="dto">Change request.</param>
        /// <param name="ct">Cancellation token.</param>
        [Authorize(Roles = "Admin")]
        [HttpPost("SitzreiheKategorieÄndern")]
        public Task<IActionResult> SitzreiheKategorieÄndern(ChangeKategorieSitzreiheDTO dto, CancellationToken ct) =>
            ExecuteAsync(async token =>
            {
                var sitzreihe = await _kinosaalService.ChangeSitzreiheKategorieAsync(dto, ct);
                return Ok(sitzreihe);
            }, ct);

        /// <summary>
        /// Updates the configured price for a seat row category.
        /// </summary>
        /// <param name="dto">Price update request.</param>
        /// <param name="ct">Cancellation token.</param>
        [Authorize(Roles = "Admin")]
        [HttpPost("KategoriePreisÄndern")]
        public Task<IActionResult> KategoriePreisÄndenr(SetPreisDTO dto, CancellationToken ct) =>
            ExecuteAsync(async token =>
            {
                await _preisService.SetPreisAsync(dto, ct);
                return Ok();
            }, ct);

        /// <summary>
        /// Deletes an auditorium by its identifier.
        /// </summary>
        /// <param name="Id">Auditorium identifier.</param>
        /// <param name="ct">Cancellation token.</param>
        [Authorize(Roles = "Admin")]
        [HttpDelete]
        public Task<IActionResult> KinosaalLöschen(long Id, CancellationToken ct) =>
            ExecuteAsync(async token =>
            {
                await _kinosaalService.DeleteAsync(Id, token);
                return new OkResult();
            }, ct);
    }
}
