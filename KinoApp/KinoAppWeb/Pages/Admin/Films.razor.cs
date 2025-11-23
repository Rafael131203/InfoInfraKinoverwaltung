using KinoAppShared.DTOs.Showtimes;
using KinoAppShared.DTOs.Vorstellung;
using KinoAppWeb.Services;
using Microsoft.AspNetCore.Components;

namespace KinoAppWeb.Pages.Admin
{
    public partial class Films : ComponentBase
    {
        [Inject] private UserSession UserSession { get; set; } = default!;
        [Inject] private NavigationManager Navigation { get; set; } = default!;
        [Inject] private IVorstellungService VorstellungService { get; set; } = default!;

        protected bool IsInitialized { get; private set; }
        protected bool IsAdmin => UserSession.IsAdmin;
        protected bool IsLoading { get; private set; }

        protected DateTime CurrentDate { get; private set; } = DateTime.Today;
        protected List<MovieShowtimeDto> Movies { get; private set; } = new();

        protected int MovieCount => Movies.Count;
        protected int ShowtimeCount => Movies.Sum(m => m.Showtimes?.Count ?? 0);

        protected override async Task OnInitializedAsync()
        {
            await UserSession.InitializeAsync();
            IsInitialized = true;

            if (!UserSession.IsAdmin)
            {
                // Non-admins get bounced to normal showtimes view
                Navigation.NavigateTo("/showtimes", true);
                return;
            }

            await LoadShowtimesFor(CurrentDate);
        }

        private async Task LoadShowtimesFor(DateTime date)
        {
            IsLoading = true;
            CurrentDate = date.Date;
            Movies.Clear();
            StateHasChanged();

            var utcDate = DateTime.SpecifyKind(date, DateTimeKind.Utc);

            var vorstellungen = await VorstellungService
                .GetVorstellungVonTagAsync(utcDate, CancellationToken.None)
                ?? new List<VorstellungDTO>();

            // Group by Film
            var grouped = vorstellungen
                .Where(v => v.Film != null)
                .GroupBy(v => v.Film!.Id);

            var list = new List<MovieShowtimeDto>();
            var uiId = 1;

            foreach (var group in grouped)
            {
                var first = group.First();
                var film = first.Film!;

                var showtimes = group
                    .OrderBy(v => v.Datum)
                    .Select(v => new ShowtimeDto
                    {
                        Id = v.Id,
                        StartsAt = v.Datum,
                        KinosaalId = v.Kinosaal?.Id ?? 0,
                        KinosaalName = v.Kinosaal?.Name ?? string.Empty
                    })
                    .ToList();

                list.Add(new MovieShowtimeDto
                {
                    Id = uiId++,
                    Title = film.Titel ?? string.Empty,
                    Description = film.Beschreibung ?? string.Empty,
                    PosterUrl = film.ImageURL ?? string.Empty,
                    DurationMinutes = film.Dauer ?? 0,
                    AgeRating = film.Fsk.HasValue ? $"FSK {film.Fsk.Value}" : "FSK",
                    Genres = film.Genre ?? string.Empty,
                    Showtimes = showtimes
                });
            }

            Movies = list
                .OrderBy(m => m.Title)
                .ToList();

            IsLoading = false;
            StateHasChanged();
        }

        protected async Task OnDateChanged(ChangeEventArgs e)
        {
            if (DateTime.TryParse(e.Value?.ToString(), out var parsed))
            {
                await LoadShowtimesFor(parsed.Date);
            }
        }

        protected async Task SetDayOffset(int offset)
        {
            var target = DateTime.Today.AddDays(offset);
            await LoadShowtimesFor(target);
        }

        protected string GetDayChipClass(int offset)
        {
            var target = DateTime.Today.AddDays(offset);
            return target.Date == CurrentDate.Date ? "chip--active" : string.Empty;
        }

        protected string FormatDuration(int minutes)
        {
            if (minutes <= 0) return "n/a";

            var h = minutes / 60;
            var m = minutes % 60;
            return h > 0 ? $"{h}h {m:D2}min" : $"{m}min";
        }

        /// <summary>
        /// Admin opens seating editor for specific film/showtime.
        /// Uses the same SelectedShowtime mechanism as the public showtimes page.
        /// </summary>
        protected async Task OpenSeating(int movieId, long showtimeId)
        {
            var movie = Movies.FirstOrDefault(m => m.Id == movieId);
            if (movie == null) return;

            var st = movie.Showtimes?.FirstOrDefault(x => x.Id == showtimeId);
            if (st == null) return;

            await UserSession.SetSelectedShowtimeAsync(
                movieId,
                movie.Title,
                movie.PosterUrl,
                st);

            Navigation.NavigateTo("/seating");
        }
    }
}
