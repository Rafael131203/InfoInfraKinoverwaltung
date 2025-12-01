using KinoAppCore.Services;
using KinoAppDB;
using KinoAppDB.Entities;
using KinoAppShared.DTOs.Authentication;
using KinoAppShared.DTOs.Kinosaal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Principal;
using System.Threading.RateLimiting;

namespace KinoAppService.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class KinosaalController : BaseController
    {
        private readonly IKinosaalService _kinosaalService;
        private readonly IPreisZuKategorieService _preisService;

        public KinosaalController(IKinosaalService kinosaalService, IPreisZuKategorieService preisService, IKinoAppDbContextScope scope)
            : base(scope)
        {
            _kinosaalService = kinosaalService;
            _preisService = preisService;
        }

        [AllowAnonymous]
        [HttpGet]
        public Task<IActionResult> GetKinosaal(long id, long? vorstellungId, CancellationToken ct) =>
            ExecuteAsync(async token =>
            {
                var kinosaal = await _kinosaalService.GetKinosaalAsync(id, vorstellungId, token);
                return Ok(kinosaal);
            }, ct);

        [Authorize(Roles = "Admin")]
        [HttpPost("Erstellen")]
        public Task<IActionResult> KinosaalErstellen(CreateKinosaalDTO dto, int AnzahlSitzreihen, int GrößeSitzreihen, CancellationToken ct) =>
            ExecuteAsync(async token =>
            {
                var id = await _kinosaalService.CreateAsync(dto, AnzahlSitzreihen, GrößeSitzreihen, token);

                return new OkObjectResult(new { id });
            }, ct);

        [Authorize(Roles = "Admin")]
        [HttpPost("SitzreiheKategorieÄndern")]
        public Task<IActionResult> SitzreiheKategorieÄndern(ChangeKategorieSitzreiheDTO dto, CancellationToken ct) =>
            ExecuteAsync(async token =>
            {
                var sitzreihe = await _kinosaalService.ChangeSitzreiheKategorieAsync(dto, ct);
                return Ok(sitzreihe);
            }, ct);

        [Authorize(Roles = "Admin")]
        [HttpPost("KategoriePreisÄndern")]
        public Task<IActionResult> KategoriePreisÄndenr(SetPreisDTO dto, CancellationToken ct) =>
            ExecuteAsync(async token =>
            {
                await _preisService.SetPreisAsync(dto, ct);
                return Ok();
            }, ct);

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
