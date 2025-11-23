using Microsoft.JSInterop;
using KinoAppShared.DTOs.Kinosaal;
using KinoAppShared.DTOs.Showtimes;
using KinoAppShared.Enums;
using KinoAppWeb.Services;
using Microsoft.AspNetCore.Components;

namespace KinoAppWeb.Pages
{
    public partial class Seating : ComponentBase
    {
        [Inject] public UserSession UserSession { get; set; } = default!;
        [Inject] public NavigationManager NavManager { get; set; } = default!;
        [Inject] public IKinosaalService KinosaalService { get; set; } = default!;
        [Inject] public IJSRuntime JS { get; set; } = default!;

        private bool _initializedJs;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && !_initializedJs)
            {
                _initializedJs = true;
                await JS.InvokeVoidAsync("KinoSeating.init");
            }
        }

        protected bool _isLoading;
        protected string? _loadError;
        protected KinosaalDTO? _kinosaal;

        protected SeatHoverInfo? _hoverSeat;

        private long _currentVorstellungId;
        protected List<SelectedSeatClientDto> _selectedSeats = new();

        protected decimal TotalPrice => _selectedSeats.Sum(s => s.Price);

        // grouped ticket summary by category
        protected IEnumerable<SeatCategorySummary> CategorySummaries =>
            _selectedSeats
                .GroupBy(s => s.Category)
                .Select(g => new SeatCategorySummary
                {
                    Category = g.Key,
                    CategoryLabel = GetCategoryLabel(g.Key),
                    Count = g.Count(),
                    TotalPrice = g.Sum(x => x.Price)
                })
                .OrderBy(c => c.Category); // Parkett, Loge, LogePlus

        protected override async Task OnInitializedAsync()
        {
            await UserSession.InitializeAsync();

            if (UserSession.SelectedShowtime is null)
            {
                NavManager.NavigateTo("/showtimes");
                return;
            }

            _currentVorstellungId = UserSession.SelectedShowtime.Showtime!.Id;

            _selectedSeats = UserSession.SelectedSeats
                .Where(s => s.VorstellungId == _currentVorstellungId)
                .ToList();

            await LoadKinosaalAsync();
        }

        private async Task LoadKinosaalAsync()
        {
            var selected = UserSession.SelectedShowtime;

            if (selected?.Showtime == null || selected.Showtime.KinosaalId <= 0)
            {
                _loadError = "No hall information available for this showtime.";
                return;
            }

            _isLoading = true;
            _loadError = null;

            try
            {
                _kinosaal = await KinosaalService.GetKinosaalAsync(
                    selected.Showtime.KinosaalId,
                    CancellationToken.None);

                if (_kinosaal == null)
                {
                    _loadError = "The hall could not be loaded.";
                }
            }
            catch (Exception ex)
            {
                _loadError = $"Error while loading hall: {ex.Message}";
            }
            finally
            {
                _isLoading = false;
                await InvokeAsync(StateHasChanged);
            }
        }

        protected async Task GoBack()
        {
            await UserSession.ClearSelectedShowtimeAsync();
            NavManager.NavigateTo("/showtimes");
        }

        protected string GetRowCategoryClass(SitzreihenKategorie category) => category switch
        {
            SitzreihenKategorie.Parkett => "seating-row--parkett",
            SitzreihenKategorie.LOGE => "seating-row--loge",
            SitzreihenKategorie.LOGEPLUS => "seating-row--logeplus",
            _ => "seating-row--parkett"
        };

        protected string GetSeatCategoryClass(SitzreihenKategorie category) => category switch
        {
            SitzreihenKategorie.Parkett => "seat--parkett",
            SitzreihenKategorie.LOGE => "seat--loge",
            SitzreihenKategorie.LOGEPLUS => "seat--logeplus",
            _ => "seat--parkett"
        };

        protected string GetCategoryLabel(SitzreihenKategorie category) => category switch
        {
            SitzreihenKategorie.Parkett => "Parkett (Standard)",
            SitzreihenKategorie.LOGE => "Loge (Premium)",
            SitzreihenKategorie.LOGEPLUS => "Loge Plus (Luxury)",
            _ => "Standard"
        };

        protected string GetSeatStateClass(SitzplatzDTO seat, bool selected)
        {
            if (seat.Gebucht)
                return "seat--taken";

            if (selected)
                return "seat--selected";

            return "seat--free";
        }

        protected bool IsSeatSelected(long seatId) =>
            _selectedSeats.Any(s => s.SeatId == seatId);

        protected async Task ToggleSeat(SitzreiheDTO row, SitzplatzDTO seat)
        {
            if (seat.Gebucht)
                return;

            var existing = _selectedSeats.FirstOrDefault(s => s.SeatId == seat.Id);
            if (existing != null)
            {
                _selectedSeats.Remove(existing);
            }
            else
            {
                _selectedSeats.Add(new SelectedSeatClientDto
                {
                    SeatId = seat.Id,
                    VorstellungId = _currentVorstellungId,
                    RowLabel = row.Bezeichnung,
                    SeatNumber = seat.Nummer,
                    Price = seat.Preis,
                    Category = row.Kategorie
                });
            }

            await UserSession.SetSelectedSeatsAsync(_selectedSeats);
            await InvokeAsync(StateHasChanged);
        }

        protected void ShowSeatInfo(SitzreiheDTO row, SitzplatzDTO seat, bool isSelected)
        {
            _hoverSeat = new SeatHoverInfo
            {
                Code = $"{row.Bezeichnung}{seat.Nummer}",
                Category = GetCategoryLabel(row.Kategorie),
                Price = seat.Preis,
                IsTaken = seat.Gebucht,
                IsSelected = isSelected
            };
        }

        protected void ClearSeatInfo() => _hoverSeat = null;

        protected string BuildSeatTitle(SitzreiheDTO row, SitzplatzDTO seat) =>
            $"{row.Bezeichnung}{seat.Nummer} · {seat.Preis:0.00} €";

        protected async Task AddToBasket()
        {
            await UserSession.SetSelectedSeatsAsync(_selectedSeats);
        }

        protected async Task GoToCheckout()
        {
            await UserSession.SetSelectedSeatsAsync(_selectedSeats);
            NavManager.NavigateTo("/checkout");
        }

        protected sealed class SeatHoverInfo
        {
            public string Code { get; set; } = string.Empty;
            public string Category { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public bool IsTaken { get; set; }
            public bool IsSelected { get; set; }
        }

        protected sealed class SeatCategorySummary
        {
            public SitzreihenKategorie Category { get; set; }
            public string CategoryLabel { get; set; } = string.Empty;
            public int Count { get; set; }
            public decimal TotalPrice { get; set; }
        }
    }
}
