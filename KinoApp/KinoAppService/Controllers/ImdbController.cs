using KinoAppCore.Components;
using KinoAppCore.Services;
using KinoAppDB;
using KinoAppShared.DTOs.Imdb;
using Microsoft.AspNetCore.Mvc;

namespace KinoAppService.Controllers
{
    /// <summary>
    /// API endpoints for querying IMDb data and reading the locally stored film catalog.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ImdbController : BaseController
    {
        private readonly IImdbService _imdbService;

        /// <summary>
        /// Creates a new <see cref="ImdbController"/>.
        /// </summary>
        /// <param name="imdbService">IMDb service used to query external data and local films.</param>
        /// <param name="scope">Database scope used for transactional execution.</param>
        public ImdbController(IImdbService imdbService, IKinoAppDbContextScope scope)
            : base(scope)
        {
            _imdbService = imdbService;
        }

        /// <summary>
        /// Returns a single movie by IMDb title identifier.
        /// </summary>
        /// <param name="id">IMDb title identifier (e.g., tt1234567).</param>
        /// <param name="ct">Cancellation token.</param>
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

        /// <summary>
        /// Lists titles from the external IMDb API using the provided request parameters.
        /// </summary>
        /// <param name="request">List/search request parameters.</param>
        /// <param name="count">Optional number of titles to import into the local catalog.</param>
        /// <param name="ct">Cancellation token.</param>
        [HttpGet("search")]
        public Task<IActionResult> Search([FromQuery] ImdbListTitlesRequest request, int? count, CancellationToken ct) =>
            ExecuteAsync(async token =>
            {
                IReadOnlyList<ImdbMovieSearchResult> results =
                    count == null
                        ? await _imdbService.ListMoviesAsync(request, token)
                        : await _imdbService.ListMoviesAsync(request, count.Value, token);

                return new OkObjectResult(results);
            }, ct);

        /// <summary>
        /// Returns all films stored in the local database.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        [HttpGet("local")]
        public Task<IActionResult> GetLocalFilms(CancellationToken ct = default) =>
            ExecuteAsync(async token =>
            {
                var films = await _imdbService.GetAllLocalFilmsAsync(token);
                return new OkObjectResult(films);
            }, ct);
    }
}
