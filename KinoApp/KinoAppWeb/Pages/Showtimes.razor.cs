using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        protected bool _isLoading = true;
        protected DateTime _currentDate = DateTime.Today;

        // IMPORTANT: initialise list here so it's never null during first render
        protected List<MovieShowtimeDto> _movies { get; set; } = new();

        protected int TotalMovies => _movies?.Count ?? 0;

        protected int TotalShowtimes =>
            _movies?.Sum(m => m.Showtimes?.Count ?? 0) ?? 0;

        protected string CurrentDateFormatted =>
            _currentDate.ToString("dddd, dd. MMMM yyyy");

        // ------------------------------------------------------------

        protected override async Task OnInitializedAsync()
        {
            await UserSession.InitializeAsync();
            await LoadShowtimesFor(_currentDate);
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
        // LOADING DATA
        // ------------------------------------------------------------

        private async Task LoadShowtimesFor(DateTime date)
        {
            _isLoading = true;
            var utcDate = DateTime.SpecifyKind(date, DateTimeKind.Utc);
            _movies.Clear();
            await InvokeAsync(StateHasChanged);

            // 1) Load Vorstellungen for this date
            var vorstellungen = await VorstellungService.GetVorstellungVonTagAsync(utcDate, CancellationToken.None)?? new List<VorstellungDTO>();

            if (vorstellungen.Count == 0)   
            {
                _isLoading = false;
                await InvokeAsync(StateHasChanged);
                return;
            }

            // 2) Group by Film (FilmDto.Id) making sure Film is not null
            var grouped = vorstellungen
                .Where(v => v.Film != null)
                .GroupBy(v => v.Film!.Id);

            var movies = new List<MovieShowtimeDto>();
            var uiId = 1;

            foreach (var group in grouped)
            {
                var first = group.FirstOrDefault();
                if (first?.Film == null)
                    continue;

                var film = first.Film; // FilmDto

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

                var movie = new MovieShowtimeDto
                {
                    Id = uiId++,                            // local UI id
                    Title = film.Titel ?? string.Empty,
                    Tagline = string.Empty,
                    Description = film.Beschreibung ?? string.Empty,
                    PosterUrl = film.ImageURL ?? string.Empty,
                    DurationMinutes = film.Dauer ?? 0,
                    AgeRating = film.Fsk.HasValue ? $"FSK {film.Fsk.Value}" : "FSK",
                    Genres = film.Genre ?? string.Empty,
                    Showtimes = showtimes
                };


                movies.Add(movie);
            }

            _movies = movies;
            _isLoading = false;
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
            await LoadShowtimesFor(DateTime.Today.AddDays(offset));
            await RefreshAnimations();
        }

        protected string GetDayChipClass(int offset)
        {
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
