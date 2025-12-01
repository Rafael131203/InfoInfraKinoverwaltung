using KinoAppShared.DTOs.Imdb;
using KinoAppShared.DTOs.Vorstellung;
using KinoAppWeb.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace KinoAppWeb.Pages
{
    public partial class Home : ComponentBase
    {
        [Inject] public IVorstellungService VorstellungService { get; set; } = default!;
        [Inject] public IJSRuntime JS { get; set; } = default!;
        [Inject] public UserSession Session { get; set; } = default!;

        protected bool _loading;
        protected string? _error;

        protected List<FilmDto> _allFilms = new();
        protected List<FilmDto> _featured = new();
        protected List<FilmDto> _grid = new();

        protected string _selectedGenre = "";
        protected string _selectedLanguage = "";   // reserved for later
        protected string _selectedSort = "SORT_BY_POPULARITY";

        protected override async Task OnInitializedAsync()
        {
            _loading = true;
            _error = null;

            try
            {
                await Session.InitializeAsync();

                // 1) Decide which date you want to show on the home page.
                //    Here: today's Vorstellungen.
                var todayLocal = DateTime.Today;
                var todayUtc = DateTime.SpecifyKind(todayLocal, DateTimeKind.Utc);

                // 2) Load Vorstellungen for this date
                var vorstellungen = await VorstellungService
                    .GetVorstellungVonTagAsync(todayUtc, CancellationToken.None);

                // 3) Extract unique films from the Vorstellungen
                _allFilms = ExtractUniqueFilmsFromVorstellungen(vorstellungen);

                // 4) Normalize Dauer as minutes (same logic as before)
                foreach (var f in _allFilms)
                {
                    if (f.Dauer.HasValue && f.Dauer.Value > 180) // assume > 3h means seconds
                    {
                        f.Dauer = f.Dauer.Value / 60;
                    }
                }

                // 5) Optionally: store in session cache so other pages (like "Now Showing")
                //    can reuse this subset of films without loading all local films.
                await Session.StoreFilmsAsync(_allFilms);

                // 6) Build featured + grid
                ApplyFilterAndSort();
            }
            catch (Exception ex)
            {
                _error = "Fehler beim Laden der Vorstellungen / Filmdaten.";
                Console.Error.WriteLine(ex);
                _allFilms.Clear();
                _featured.Clear();
                _grid.Clear();
            }
            finally
            {
                _loading = false;
            }
        }


        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            // only initialize the hero slider JS once
            if (firstRender && _featured.Count > 0)
            {
                try
                {
                    await JS.InvokeVoidAsync("kinoInitHero");
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"JS init error: {ex}");
                }
            }
        }

        private void ApplyFilterAndSort()
        {
            IEnumerable<FilmDto> query = _allFilms;

            // GENRE FILTER (simple contains on Genre string)
            if (!string.IsNullOrWhiteSpace(_selectedGenre))
            {
                query = query.Where(m =>
                    !string.IsNullOrWhiteSpace(m.Genre) &&
                    m.Genre.Contains(_selectedGenre, StringComparison.OrdinalIgnoreCase));
            }

            // LANGUAGE FILTER placeholder – apply once FilmDto has a language field

            // SORTING – for now, we use Titel (you can change later)
            query = _selectedSort switch
            {
                "SORT_BY_RELEASE_DATE" => query.OrderBy(m => m.Titel),
                "SORT_BY_USER_RATING" => query.OrderBy(m => m.Titel),
                "SORT_BY_USER_RATING_COUNT" => query.OrderBy(m => m.Titel),
                "SORT_BY_YEAR" => query.OrderBy(m => m.Titel),
                _ => query.OrderBy(m => m.Titel)
            };

            var ordered = query.ToList();

            _featured = ordered.Take(3).ToList();
            _grid = ordered.Skip(3).Take(12).ToList();
        }

        protected Task OnGenreChanged(ChangeEventArgs e)
        {
            _selectedGenre = e.Value?.ToString() ?? "";
            ApplyFilterAndSort();
            return InvokeAsync(StateHasChanged);
        }

        protected Task OnLanguageChanged(ChangeEventArgs e)
        {
            _selectedLanguage = e.Value?.ToString() ?? "";
            // currently no language filter logic
            ApplyFilterAndSort();
            return InvokeAsync(StateHasChanged);
        }

        protected Task OnSortChanged(ChangeEventArgs e)
        {
            _selectedSort = e.Value?.ToString() ?? "SORT_BY_POPULARITY";
            ApplyFilterAndSort();
            return InvokeAsync(StateHasChanged);
        }

        // Helper for runtime in minutes (for Razor)
        protected static int? GetRuntimeMinutes(FilmDto film)
        {
            return film.Dauer;
        }

        private static List<FilmDto> ExtractUniqueFilmsFromVorstellungen(List<VorstellungDTO> vorstellungen)
        {
            if (vorstellungen == null || vorstellungen.Count == 0)
                return new List<FilmDto>();

            return vorstellungen
                .Where(v => v.Film != null)
                .GroupBy(v => v.Film!.Id)
                .Select(g => g.First().Film!)
                .ToList();
        }

    }
}
