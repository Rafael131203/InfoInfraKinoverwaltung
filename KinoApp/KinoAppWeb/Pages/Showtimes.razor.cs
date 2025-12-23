using KinoAppShared.DTOs.Showtimes;
using KinoAppShared.DTOs.Vorstellung;
using KinoAppWeb.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace KinoAppWeb.Pages
{
    public partial class Showtimes : ComponentBase
    {
        [Inject] public IVorstellungService VorstellungService { get; set; } = default!;
        [Inject] public UserSession UserSession { get; set; } = default!;
        [Inject] public NavigationManager Nav { get; set; } = default!;
        [Inject] public IJSRuntime JS { get; set; } = default!;

        protected bool _isLoading = false;
        protected bool _hasLoadedOnce = false;

        protected DateTime _currentDate = DateTime.Today;

        // initialise list here so it's never null during first render
        protected List<MovieShowtimeDto> _movies { get; set; } = new();

        protected int TotalMovies => _movies?.Count ?? 0;

        protected int TotalShowtimes =>
            _movies?.Sum(m => m.Showtimes?.Count ?? 0) ?? 0;

        protected string CurrentDateFormatted => _currentDate.ToString("dddd, dd. MMMM yyyy");


        // ------------------------------------------------------------

        protected override async Task OnInitializedAsync()
        {
            await UserSession.InitializeAsync();

            _movies.Clear();

            // Auto-load today's showtimes so it's consistent with Home showing today's content
            await LoadShowtimesFor(DateTime.Today);
            _hasLoadedOnce = true;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                try
                {
                    await JS.InvokeVoidAsync("KinoShowtimes.init");
                }
                catch
                {
                    // ignore JS errors
                }
            }
        }

        // ------------------------------------------------------------
        // LOADING DATA (with retry)
        // ------------------------------------------------------------

        private async Task LoadShowtimesFor(DateTime date)
        {
            _isLoading = true;
            _currentDate = date.Date;
            _movies.Clear();
            await InvokeAsync(StateHasChanged);

            var utcDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);

            // Single call – just like admin
            var vorstellungen = await VorstellungService
                .GetVorstellungVonTagAsync(utcDate, CancellationToken.None)
                ?? new List<VorstellungDTO>();

            // Group by Film
            var grouped = vorstellungen
                .Where(v => v.Film != null)
                .GroupBy(v => v.Film!.Id);

            var movies = new List<MovieShowtimeDto>();
            var uiId = 1;

            foreach (var group in grouped)
            {
                var first = group.First();
                var film = first.Film!;

                // Normalize runtime: seconds → minutes if needed
                var durationMinutes = film.Dauer ?? 0;
                if (durationMinutes > 180)
                {
                    durationMinutes /= 60;
                }

                var showtimes = group
                    .OrderBy(v => v.Datum)
                    .Select(v => new ShowtimeDto
                    {
                        Id = v.Id,                          // Vorstellung Id
                        StartsAt = v.Datum,                 // Vorstellung.Datum
                        KinosaalId = v.Kinosaal?.Id ?? 0,
                        KinosaalName = v.Kinosaal?.Name ?? string.Empty
                    })
                    .ToList();

                movies.Add(new MovieShowtimeDto
                {
                    Id = uiId++,                            // local UI id
                    Title = film.Titel ?? string.Empty,
                    Tagline = string.Empty,
                    Description = film.Beschreibung ?? string.Empty,
                    PosterUrl = film.ImageURL ?? string.Empty,
                    DurationMinutes = durationMinutes,
                    AgeRating = film.Fsk.HasValue ? $"FSK {film.Fsk.Value}" : "FSK",
                    Genres = film.Genre ?? string.Empty,
                    Showtimes = showtimes
                });
            }

            // Order like admin
            _movies = movies
                .OrderBy(m => m.Title)
                .ToList();

            _isLoading = false;
            _hasLoadedOnce = true;

            await RefreshAnimations();        // keep your JS animation
            await InvokeAsync(StateHasChanged);
        }


        // ------------------------------------------------------------
        // DATE CONTROLS
        // ------------------------------------------------------------

        protected async Task OnDateChanged(ChangeEventArgs e)
        {
            if (DateTime.TryParse(e.Value?.ToString(), out var parsed))
            {
                await LoadShowtimesFor(parsed.Date);
                await RefreshAnimations();
            }
        }

        protected async Task SetDayOffset(int offset)
        {
            var target = DateTime.Today.AddDays(offset);
            await LoadShowtimesFor(target);
            await RefreshAnimations();
        }

        protected string GetDayChipClass(int offset)
        {
            if (!_hasLoadedOnce)
                return string.Empty;

            var target = DateTime.Today.AddDays(offset);
            return target.Date == _currentDate.Date ? "chip--active" : string.Empty;
        }

        // ------------------------------------------------------------
        // HELPERS
        // ------------------------------------------------------------

        protected string FormatDuration(int minutes)
        {
            if (minutes <= 0) return "n/a";

            var h = minutes / 60;
            var m = minutes % 60;

            return h > 0 ? $"{h}h {m:D2}min" : $"{m}min";
        }

        // ------------------------------------------------------------
        // CLICK HANDLERS
        // ------------------------------------------------------------

        protected void GoToMovie(int movieId)
        {
            // optional: no-op for now
        }

        /// <summary>
        /// When a showtime chip is clicked:
        /// - find the movie + showtime
        /// - store in UserSession.SelectedShowtime
        /// - navigate to /seating
        /// </summary>
        protected async Task GoToSeating(int movieId, long showtimeId)
        {
            var movie = _movies.FirstOrDefault(m => m.Id == movieId);
            if (movie == null) return;

            var showtime = movie.Showtimes?.FirstOrDefault(s => s.Id == showtimeId);
            if (showtime == null) return;

            await UserSession.SetSelectedShowtimeAsync(
                movieId,
                movie.Title,
                movie.PosterUrl,
                showtime
            );

            Nav.NavigateTo("/seating");
        }

        protected void GoToEditMovies()
        {
            Nav.NavigateTo("/admin/films");
        }

        private async Task RefreshAnimations()
        {
            try
            {
                await JS.InvokeVoidAsync("KinoShowtimes.refresh");
            }
            catch
            {
                // ignore
            }
        }
    }
}
