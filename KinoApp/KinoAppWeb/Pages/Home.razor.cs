using KinoAppCore.Components;      // ImdbMovieSearchResult
using KinoAppWeb.Services;         // ImdbApiClient
using Microsoft.AspNetCore.Components;

namespace KinoAppWeb.Pages
{
    public partial class Home : ComponentBase
    {
        [Inject] public ImdbApiClient Imdb { get; set; } = default!;

        protected bool _loading;
        protected string? _error;

        // Movies for hero slider + grid
        protected List<ImdbMovieSearchResult> _featured = new();
        protected List<ImdbMovieSearchResult> _grid = new();

        protected override async Task OnInitializedAsync()
        {
            _loading = true;
            _error = null;

            try
            {
                // TODO: change this search term to whatever you like
                var results = await Imdb.SearchAsync("cinema");

                if (results.Count >= 3)
                {
                    _featured = results.Take(3).ToList();
                    _grid = results.Skip(3).Take(4).ToList();
                }
                else
                {
                    _featured = results.ToList();
                    _grid.Clear();
                }
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
            }
        }
    }
}
