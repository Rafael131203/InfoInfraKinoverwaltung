using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KinoAppService.Controllers
{
    using KinoAppCore.Services;
    using KinoAppShared.Messaging;
    using MongoDB.Bson;
    using MongoDB.Driver;

    [ApiController]
    [Route("api/[controller]")]
    public class StatsController : ControllerBase
    {
        private readonly TicketService _ticketService;

        public StatsController(TicketService ticketService)
        {
            _ticketService = ticketService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var allStats = await _ticketService.GetAllAsync();
            return Ok(allStats);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TicketStat stat)
        {
            await _ticketService.CreateAsync(stat);
            return Ok(stat);
        }
    }



}
