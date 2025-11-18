using KinoAppCore.Components;
using KinoAppCore.Services;
using KinoAppDB;
using KinoAppShared.DTOs.Imdb;
using Microsoft.AspNetCore.Mvc;

namespace KinoAppService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImdbController : BaseController
    {
        private readonly IImdbService _imdbService;

        public ImdbController(IImdbService imdbService, IKinoAppDbContextScope scope)
            : base(scope)
        {
            _imdbService = imdbService;
        }

        // GET api/imdb/{id}
        [HttpGet("{id}")]
        public Task<IActionResult> GetById(string id, CancellationToken ct) =>
            ExecuteAsync(async token =>
            {
                if (string.IsNullOrWhiteSpace(id))
                    return new BadRequestObjectResult("id is required.");

                ImdbMovieDetails? movie = await _imdbService.GetMovieByImdbIdAsync(id, token);
                if (movie == null)
                    return new NotFoundResult();

                return new OkObjectResult(movie);
            }, ct);

        // GET api/imdb/search?query=Inception
        [HttpGet("search")]
        public Task<IActionResult> Search([FromQuery] ImdbListTitlesRequest request, CancellationToken ct) =>
            ExecuteAsync(async token =>
            {
                // request comes in with all query parameters bound automatically
                var results = await _imdbService.ListMoviesAsync(request, token);
                return new OkObjectResult(results);
            }, ct);
    }
}
