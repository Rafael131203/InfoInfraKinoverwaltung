using KinoAppDB;
using KinoAppCore.Services;
using KinoAppShared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace KinoAppService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KundenController : BaseController
    {
        private readonly IKundeService _kundenService;

        public KundenController(IKundeService kundenService, IKinoAppDbContextScope scope) : base(scope)
        {
            _kundenService = kundenService;
        }

        // GET api/kunden/5
        [HttpGet("{id:long}")]
        public Task<IActionResult> Get(long id, CancellationToken ct) =>
            ExecuteAsync(async token =>
            {
                var dto = await _kundenService.GetAsync(id, token);
                return dto is null ? new NotFoundResult() : new OkObjectResult(dto);
            }, ct);

        // GET api/kunden
        [HttpGet]
        public Task<IActionResult> GetAll(CancellationToken ct) =>
            ExecuteAsync(async token =>
            {
                var list = await _kundenService.GetAllAsync(token);
                return new OkObjectResult(list);
            }, ct);

        // POST api/kunden
        [HttpPost]
        public Task<IActionResult> Post([FromBody] FullKundeDTO kundeDTO, CancellationToken ct) =>
            ExecuteAsync(async token =>
            {
                if (kundeDTO is null)
                    return new BadRequestResult();

                var created = await _kundenService.CreateAsync(kundeDTO, token);
                return new CreatedAtActionResult(
                    nameof(Get),
                    "Kunden",
                    new { id = created.Id },
                    created
                );
            }, ct);

        // PUT api/kunden/5
        [HttpPut("{id:long}")]
        public Task<IActionResult> Put(long id, [FromBody] FullKundeDTO kundeDTO, CancellationToken ct) =>
            ExecuteAsync(async token =>
            {
                if (kundeDTO is null)
                    return new BadRequestResult();

                var updated = await _kundenService.UpdateAsync(id, kundeDTO, token);
                if (updated is null)
                    return new NotFoundResult();

                return new OkObjectResult(updated);
            }, ct);

        // DELETE api/kunden/5
        [HttpDelete("{id:long}")]
        public Task<IActionResult> Delete(long id, CancellationToken ct) =>
            ExecuteAsync(async token =>
            {
                var success = await _kundenService.DeleteAsync(id, token);
                return success ? new NoContentResult() : new NotFoundResult();
            }, ct);
    }
}
