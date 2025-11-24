using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KinoAppService.Controllers
{
    using KinoAppCore.Documents;
    using KinoAppCore.Services;
    using KinoAppShared.Messaging;
    using MongoDB.Bson;
    using MongoDB.Driver;

    [ApiController]
    [Route("api/[controller]")]
    public class StatsController : ControllerBase
    {
        private readonly StatsService _statsService;

        public StatsController(StatsService statsService)
        {
            _statsService = statsService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var allStats = await _statsService.GetAllAsync();
            return Ok(allStats);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] DailyShowRevenue stat)
        {
            // 2. ÄNDERUNG: Aufruf geht jetzt an den StatsService
            await _statsService.CreateAsync(stat);
            return Ok(stat);
        }
    }



}
