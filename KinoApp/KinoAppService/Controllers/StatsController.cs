using KinoAppCore.Documents;
using KinoAppCore.Services;
using Microsoft.AspNetCore.Mvc;

namespace KinoAppService.Controllers
{
    /// <summary>
    /// API endpoints for accessing stored statistics and projections.
    /// </summary>
    /// <remarks>
    /// This controller exposes read/write access to the MongoDB-backed statistics store.
    /// In production scenarios, write endpoints are often limited to internal tooling or background processing.
    /// </remarks>
    [ApiController]
    [Route("api/[controller]")]
    public class StatsController : ControllerBase
    {
        private readonly StatsService _statsService;

        /// <summary>
        /// Creates a new <see cref="StatsController"/>.
        /// </summary>
        /// <param name="statsService">Service used to access the statistics collection.</param>
        public StatsController(StatsService statsService)
        {
            _statsService = statsService;
        }

        /// <summary>
        /// Returns all statistics documents.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var allStats = await _statsService.GetAllAsync();
            return Ok(allStats);
        }

        /// <summary>
        /// Inserts a statistics document.
        /// </summary>
        /// <param name="stat">Document to insert.</param>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] DailyShowRevenue stat)
        {
            await _statsService.CreateAsync(stat);
            return Ok(stat);
        }
    }
}
