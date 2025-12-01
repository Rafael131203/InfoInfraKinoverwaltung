using KinoAppShared.DTOs;
using KinoAppShared.DTOs.Kinosaal;
using KinoAppShared.DTOs.Showtimes;
using KinoAppShared.DTOs.Ticket;
using KinoAppShared.Enums;
using KinoAppWeb.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Linq;

namespace KinoAppWeb.Pages
{
    public partial class Seating : ComponentBase
    {
        [Inject] public UserSession UserSession { get; set; } = default!;
        [Inject] public NavigationManager NavManager { get; set; } = default!;
        [Inject] public IKinosaalService KinosaalService { get; set; } = default!;
        [Inject] public ITicketApiService TicketService { get; set; } = default!;
        [Inject] public IJSRuntime JS { get; set; } = default!;

        private bool _initializedJs;

        protected bool _isLoading;
        protected string? _loadError;
        protected string? _reserveError;
        protected KinosaalDTO? _kinosaal;

        protected SeatHoverInfo? _hoverSeat;

        private long _currentVorstellungId;
        protected List<SelectedSeatClientDto> _selectedSeats = new();

        protected decimal TotalPrice => _selectedSeats.Sum(s => s.Price);

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
                .OrderBy(c => c.Category);

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && !_initializedJs)
            {
                _initializedJs = true;
                await JS.InvokeVoidAsync("KinoSeating.init");
            }
        }

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
            _reserveError = null;

            try
            {
                _kinosaal = await KinosaalService.GetKinosaalAsync(
                    selected.Showtime.KinosaalId,
                    _currentVorstellungId,
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
            // Status 0 = Nothing -> hidden (we also skip rendering in Razor)
            if (seat.Status == TicketStatus.Nothing)
                return "seat--hidden";

            // Reserved or Booked => show as taken (gray)
            if (seat.Status == TicketStatus.Reserved || seat.Status == TicketStatus.Booked)
                return "seat--taken";

            if (selected)
                return "seat--selected";

            return "seat--free";
        }

        protected bool IsSeatSelected(long seatId) =>
            _selectedSeats.Any(s => s.SeatId == seatId);

        protected async Task ToggleSeat(SitzreiheDTO row, SitzplatzDTO seat)
        {
            // Cannot modify seats that are Reserved/Booked or Nothing
            if (seat.Status == TicketStatus.Reserved ||
                seat.Status == TicketStatus.Booked ||
                seat.Status == TicketStatus.Nothing)
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

            _reserveError = null;
            await UserSession.SetSelectedSeatsAsync(_selectedSeats);
            await InvokeAsync(StateHasChanged);
        }

        protected void ShowSeatInfo(SitzreiheDTO row, SitzplatzDTO seat, bool isSelected)
        {
            var isTaken = seat.Status == TicketStatus.Reserved || seat.Status == TicketStatus.Booked;

            _hoverSeat = new SeatHoverInfo
            {
                Code = $"{row.Bezeichnung}{seat.Nummer}",
                Category = GetCategoryLabel(row.Kategorie),
                Price = seat.Preis,
                IsTaken = isTaken,
                IsSelected = isSelected
            };
        }

        protected void ClearSeatInfo() => _hoverSeat = null;

        protected string BuildSeatTitle(SitzreiheDTO row, SitzplatzDTO seat) =>
            $"{row.Bezeichnung}{seat.Nummer} · {seat.Preis:0.00} €";

        // ------------------- RESERVATION HELPERS ------------------------

        private async Task<bool> ReserveCurrentSelectionAsync()
        {
            _reserveError = null;

            if (!_selectedSeats.Any())
                return true; // nothing to reserve

            var request = new ReserveTicketDTO
            {
                VorstellungId = _currentVorstellungId,
                SitzplatzIds = _selectedSeats.Select(s => s.SeatId).ToList()
            };

            try
            {
                var token = await UserSession.GetValidAccessTokenAsync();
                await TicketService.ReserveTicketsAsync(request, token);

                // 🔥 Immediately reload hall so the new Reserved state is visible
                await LoadKinosaalAsync();

                return true;
            }
            catch (Exception ex)
            {
                _reserveError = ex.Message;
                return false;
            }
            finally
            {
                await InvokeAsync(StateHasChanged);
            }
        }
        protected async Task AddToBasket()
        {
            var success = await ReserveCurrentSelectionAsync();
            if (!success) return;

            // 🔥 add current selection to cart
            await UserSession.AddToCartAsync(_selectedSeats);

            // clear selection (both in memory and storage)
            _selectedSeats.Clear();
            await UserSession.ClearSelectedSeatsAsync();

            // optional: reload hall so reserved seats show immediately
            await LoadKinosaalAsync();
        }

        protected async Task GoToCheckout()
        {
            var success = await ReserveCurrentSelectionAsync();
            if (!success) return;

            // 🔥 add current selection to cart
            await UserSession.AddToCartAsync(_selectedSeats);

            // clear selection
            _selectedSeats.Clear();
            await UserSession.ClearSelectedSeatsAsync();

            NavManager.NavigateTo("/checkout");
        }


        // ----------------------------------------------------------------

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
