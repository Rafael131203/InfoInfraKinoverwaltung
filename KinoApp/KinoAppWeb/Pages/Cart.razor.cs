using KinoAppShared.DTOs.Kinosaal;
using KinoAppWeb.Services;
using Microsoft.AspNetCore.Components;

namespace KinoAppWeb.Pages
{
    public partial class Cart : ComponentBase
    {
        [Inject] public UserSession UserSession { get; set; } = default!;
        [Inject] public NavigationManager Nav { get; set; } = default!;

        public List<CartGroupDto> Groups { get; private set; } = new();
        public bool IsEmpty => Groups.Count == 0;
        public decimal OverallTotal => Groups.Sum(x => x.Total);

        protected override async Task OnInitializedAsync()
        {
            await UserSession.InitializeAsync();
            BuildGroupedCart();
        }

        private void BuildGroupedCart()
        {
            Groups = new();

            if (!UserSession.CartItems.Any())
                return;

            // Group by Vorstellung
            var grouped = UserSession.CartItems
                .GroupBy(x => x.Showtime.Id)
                .ToList();

            foreach (var g in grouped)
            {
                var first = g.First();

                Groups.Add(new CartGroupDto
                {
                    VorstellungId = first.Showtime.Id,
                    MovieTitle = first.MovieTitle,
                    PosterUrl = first.PosterUrl,
                    StartTimeString = first.Showtime.StartsAt.ToString("dddd, dd.MM.yyyy HH:mm"),
                    KinosaalName = first.Showtime.KinosaalName,
                    Seats = g.Select(x => x.Seat)
                             .OrderBy(s => s.RowLabel)
                             .ThenBy(s => s.SeatNumber)
                             .ToList(),
                    Total = g.Sum(x => x.Seat.Price)
                });
            }
        }

        protected void GoToShowtimes() => Nav.NavigateTo("/showtimes");
        protected void GoToCheckout() => Nav.NavigateTo("/checkout");
    }

    public class CartGroupDto
    {
        public long VorstellungId { get; set; }
        public string MovieTitle { get; set; } = "";
        public string? PosterUrl { get; set; }
        public string StartTimeString { get; set; } = "";
        public string? KinosaalName { get; set; }
        public List<SelectedSeatClientDto> Seats { get; set; } = new();
        public decimal Total { get; set; }
    }
}
