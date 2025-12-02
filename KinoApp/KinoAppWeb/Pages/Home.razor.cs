using KinoAppShared.DTOs.Imdb;
using KinoAppWeb.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace KinoAppWeb.Pages
{
    public partial class Home : ComponentBase
    {
        [Inject] public ImdbApiClient Imdb { get; set; } = default!;
        [Inject] public IJSRuntime JS { get; set; } = default!;
        [Inject] public UserSession Session { get; set; } = default!;

        protected bool _loading;
        protected string? _error;

        protected List<FilmDto> _allFilms = new();
        protected List<FilmDto> _featured = new();
        protected List<FilmDto> _grid = new();

        protected string _selectedGenre = "";
        protected string _selectedSort = "SORT_BY_POPULARITY";

        protected override async Task OnInitializedAsync()
        {
            _loading = true;
            _error = null;

            try
            {
                await Session.InitializeAsync();

                //
                // 1️⃣ Load all films (cached if available)
                //
                _allFilms = await Session.GetFilmsAsync(Imdb);

                //
                // 2️⃣ Normalize Dauer (because DB stores seconds)
                //
                foreach (var f in _allFilms)
                {
                    if (f.Dauer.HasValue && f.Dauer.Value > 180)
                        f.Dauer /= 60;
                }

                //
                // 3️⃣ Build slider + grid
                //
                ApplyFilterAndSort();
            }
            catch (Exception ex)
            {
                _error = "Fehler beim Laden der Filmdaten.";
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

        private void ApplyFilterAndSort()
        {
            IEnumerable<FilmDto> query = _allFilms;

            // GENRE FILTER
            if (!string.IsNullOrWhiteSpace(_selectedGenre))
            {
                query = query.Where(m =>
                    !string.IsNullOrWhiteSpace(m.Genre) &&
                    m.Genre.Contains(_selectedGenre, StringComparison.OrdinalIgnoreCase));
            }

            // SORTING
            query = _selectedSort switch
            {
                _ => query.OrderBy(m => m.Titel)
            };

            var ordered = query.ToList();

            _featured = ordered.Take(3).ToList();
            _grid = ordered.Skip(3).Take(20).ToList();
        }
    }
}
