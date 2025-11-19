using KinoAppCore.Services;
using KinoAppDB;
using KinoAppShared.DTOs.Authentication;
using KinoAppShared.DTOs.Kinosaal;
using Microsoft.AspNetCore.Mvc;

namespace KinoAppService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KinosaalController : BaseController
    {
        private readonly IKinosaalService _kinosaalService;

        public KinosaalController(IKinosaalService kinosaalService, IKinoAppDbContextScope scope)
            : base(scope)
        {
            _kinosaalService = kinosaalService;
        }

        [HttpPost("Erstellen")]
        public async Task<IActionResult> KinosaalErstellen(CreateKinosaalDTO dto, int AnzahlSitzreihen, int GrößeSitzreihen, CancellationToken ct)
        {
            await _kinosaalService.CreateAsync(dto, AnzahlSitzreihen, GrößeSitzreihen, ct);
            return Ok();
        }

    }
}
