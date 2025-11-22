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
        public Task<IActionResult> Search([FromQuery] ImdbListTitlesRequest request, int? count,CancellationToken ct) =>
            ExecuteAsync(async token =>
            {
                IReadOnlyList<ImdbMovieSearchResult> results;

                if (count == null)
                {
                    results = await _imdbService.ListMoviesAsync(request, token);
                }
                else
                {
                    results = await _imdbService.ListMoviesAsync(request, count.Value, token);
                }

                return new OkObjectResult(results);
            }, ct);

        [HttpGet("local")]
        public Task<IActionResult> GetLocalFilms(CancellationToken ct = default) =>
            ExecuteAsync(async token =>
            {
                var films = await _imdbService.GetAllLocalFilmsAsync(token);
                return new OkObjectResult(films);
            }, ct);



    }
}
