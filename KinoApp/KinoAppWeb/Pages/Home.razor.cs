using KinoAppCore.Components;
using KinoAppWeb.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace KinoAppWeb.Pages
{
    public partial class Home : ComponentBase
    {
        [Inject] public ImdbApiClient Imdb { get; set; } = default!;
        [Inject] public IJSRuntime JS { get; set; } = default!;

        protected bool _loading;
        protected string? _error;

        protected List<ImdbMovieSearchResult> _featured = new();
        protected List<ImdbMovieSearchResult> _grid = new();

        protected string _selectedGenre = "";
        protected string _selectedLanguage = "";
        protected string _selectedSort = "SORT_BY_POPULARITY";

        protected override async Task OnInitializedAsync()
        {
            await ReloadMoviesAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            // As soon as we *have* featured movies, (re)initialize JS slider.
            if (_featured is { Count: > 0 })
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

        protected async Task ReloadMoviesAsync()
        {
            _loading = true;
            _error = null;
            StateHasChanged();

            try
            {
                var results = await Imdb.SearchFilteredAsync(
                    genre: _selectedGenre,
                    languageCode: _selectedLanguage,
                    sortBy: "SORT_BY_USER_RATING",   // top rated
                    sortOrder: "DESC"
                );

                var ordered = results
                    .OrderByDescending(m => m.Rating ?? 0.0)
                    .ToList();

                _featured = ordered.Take(3).ToList();
                _grid = ordered.Skip(3).Take(12).ToList();
            }
            catch (Exception ex)
            {
                _error = "Fehler beim Laden der Filmdaten.";
                Console.Error.WriteLine(ex);
                _featured.Clear();
                _grid.Clear();
            }
            finally
            {
                _loading = false;
                StateHasChanged();
            }
        }

        protected async Task OnGenreChanged(ChangeEventArgs e)
        {
            _selectedGenre = e.Value?.ToString() ?? "";
            await ReloadMoviesAsync();
        }

        protected async Task OnLanguageChanged(ChangeEventArgs e)
        {
            _selectedLanguage = e.Value?.ToString() ?? "";
            await ReloadMoviesAsync();
        }

        protected async Task OnSortChanged(ChangeEventArgs e)
        {
            _selectedSort = e.Value?.ToString() ?? "SORT_BY_POPULARITY";
            await ReloadMoviesAsync();
        }
    }
}
