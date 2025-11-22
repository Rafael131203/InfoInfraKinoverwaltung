using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KinoAppShared.DTOs.Showtimes;
using KinoAppCore.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace KinoAppWeb.Pages
{
    public partial class Showtimes : ComponentBase
    {
        [Inject] private IMovieShowtimeService MovieShowtimeService { get; set; } = default!;
        [Inject] private NavigationManager NavManager { get; set; } = default!;
        [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

        protected bool _isLoading = true;
        protected List<MovieShowtimeDto> _movies = new();
        protected DateTime _currentDate = DateTime.Today;

        protected int TotalMovies => _movies.Count;
        protected int TotalShowtimes => _movies.Sum(m => m.Showtimes?.Count ?? 0);

        protected string CurrentDateFormatted =>
            _currentDate.ToString("dddd, dd. MMMM yyyy");

        protected override async Task OnInitializedAsync()
        {
            await LoadShowtimesFor(_currentDate);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                // Initialize JS animations for the cards
                try
                {
                    await JSRuntime.InvokeVoidAsync("KinoShowtimes.init");
                }
                catch
                {
                    // non-fatal if JS is not loaded
                }
            }
        }

        protected async Task LoadShowtimesFor(DateTime date)
        {
            _isLoading = true;
            _currentDate = date;

            var items = await MovieShowtimeService.GetByDateAsync(date);
            _movies = items?.ToList() ?? new List<MovieShowtimeDto>();

            _isLoading = false;
            await InvokeAsync(StateHasChanged);
        }

        protected async Task OnDateChanged(ChangeEventArgs e)
        {
            if (DateTime.TryParse(e.Value?.ToString(), out var parsed))
            {
                _currentDate = parsed.Date;
                await LoadShowtimesFor(_currentDate);
                await RefreshAnimations();
            }
        }


        protected async Task SetDayOffset(int offset)
        {
            _currentDate = DateTime.Today.AddDays(offset);

            await LoadShowtimesFor(_currentDate);
            await RefreshAnimations();
        }


        protected string GetDayChipClass(int offset)
        {
            var day = DateTime.Today.AddDays(offset);
            return day.Date == _currentDate.Date ? "chip--active" : string.Empty;
        }

        protected string FormatDuration(int minutes)
        {
            if (minutes <= 0) return "n/a";

            var h = minutes / 60;
            var m = minutes % 60;

            return h > 0 ? $"{h}h {m:D2}min" : $"{m}min";
        }

        protected void GoToMovie(int movieId)
        {
            NavManager.NavigateTo($"/seating/{movieId}");
        }

        protected void GoToSeating(int movieId, int showtimeId)
        {
            NavManager.NavigateTo($"/seating/{movieId}?showtimeId={showtimeId}");
        }

        private async Task RefreshAnimations()
        {
            try
            {
                await JSRuntime.InvokeVoidAsync("KinoShowtimes.refresh");
            }
            catch
            {
                // ignore if JS not present
            }
        }
    }
}
