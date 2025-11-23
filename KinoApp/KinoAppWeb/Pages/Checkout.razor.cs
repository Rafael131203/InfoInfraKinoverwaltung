using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KinoAppShared.DTOs.Showtimes;
using KinoAppWeb.Services;
using Microsoft.AspNetCore.Components;

namespace KinoAppWeb.Pages
{
    public partial class Checkout : ComponentBase
    {
        [Inject] public UserSession UserSession { get; set; } = default!;
        [Inject] public NavigationManager Nav { get; set; } = default!;

        // Exposed to the .razor markup
        public IReadOnlyList<SelectedSeatClientDto> Seats { get; private set; }
            = new List<SelectedSeatClientDto>();

        public decimal Total { get; private set; }

        protected string? _confirmationMessage;

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

            Seats = UserSession.SelectedSeats
                .Where(x => x.VorstellungId == s.Showtime!.Id)
                .OrderBy(x => x.RowLabel)
                .ThenBy(x => x.SeatNumber)
                .ToList();

            Total = Seats.Sum(x => x.Price);
        }

        protected void BackToSeats()
        {
            Nav.NavigateTo("/seating");
        }

        protected void Confirm()
        {
            // Placeholder – here you would call your booking API.
            _confirmationMessage = "This is where your real booking call would happen.";
        }
    }
}
