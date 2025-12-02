using System.Globalization;
using KinoAppShared.DTOs;
using KinoAppShared.DTOs.Showtimes;
using KinoAppWeb.Services;
using Microsoft.AspNetCore.Components;

namespace KinoAppWeb.Pages
{
    public partial class Checkout : ComponentBase
    {
        [Inject] public UserSession UserSession { get; set; } = default!;
        [Inject] public NavigationManager Nav { get; set; } = default!;
        [Inject] public ITicketApiService TicketService { get; set; } = default!;

        // Data for the View
        public IReadOnlyList<SelectedSeatClientDto> Seats { get; private set; }
            = new List<SelectedSeatClientDto>();
        public decimal Total { get; private set; }

        // Form Fields
        public string? GuestEmail { get; set; }

        // State Flags
        protected bool _isBusy;
        protected bool _isSuccess;
        protected string? _errorMessage;

        protected override async Task OnInitializedAsync()
        {
            await UserSession.InitializeAsync();

            if (UserSession.SelectedShowtime is null)
            {
                Seats = new List<SelectedSeatClientDto>();
                Total = 0m;
                return;
            }

            var s = UserSession.SelectedShowtime;

            // 🔥 use Cart, but only seats for this showtime
            Seats = UserSession.CartItems
                .Where(x => x.Showtime.Id == s.Showtime!.Id)
                .OrderBy(x => x.Seat.RowLabel)
                .ThenBy(x => x.Seat.SeatNumber)
                .Select(x => x.Seat)                 // 🔥 convert to SelectedSeatClientDto
                .ToList();



            Total = Seats.Sum(x => x.Price);

            if (UserSession.IsAuthenticated && !string.IsNullOrEmpty(UserSession.Current?.Email))
            {
                GuestEmail = UserSession.Current.Email;
            }
        }

        protected void BackToSeats()
        {
            Nav.NavigateTo("/seating");
        }

        protected async Task Confirm()
        {
            if (_isBusy) return;
            _isBusy = true;
            _errorMessage = null;

            try
            {
                if (UserSession.SelectedShowtime?.Showtime == null || !Seats.Any())
                {
                    _errorMessage = "Your cart is empty.";
                    return;
                }

                if (!UserSession.IsAuthenticated && string.IsNullOrWhiteSpace(GuestEmail))
                {
                    _errorMessage = "Please enter an email address for your tickets.";
                    return;
                }

                var request = new BuyTicketDTO
                {
                    VorstellungId = UserSession.SelectedShowtime.Showtime.Id,
                    SitzplatzIds = Seats.Select(s => s.SeatId).ToList(),
                    GastEmail = !UserSession.IsAuthenticated ? GuestEmail : null
                };

                var token = await UserSession.GetValidAccessTokenAsync();
                await TicketService.BuyTicketAsync(request, token);

                _isSuccess = true;

                // 🔥 clear cart + showtime + any leftover selection
                await UserSession.ClearCartAsync();
                await UserSession.ClearSelectedSeatsAsync();
                await UserSession.ClearSelectedShowtimeAsync();
            }
            catch (Exception ex)
            {
                _errorMessage = ex.Message;
            }
            finally
            {
                _isBusy = false;
            }
        }

    }
}