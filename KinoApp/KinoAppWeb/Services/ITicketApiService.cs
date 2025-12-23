using KinoAppShared.DTOs;
using KinoAppShared.DTOs.Ticket;

namespace KinoAppWeb.Services
{
    /// <summary>
    /// Client-side API wrapper for ticket purchase and reservation endpoints.
    /// </summary>
    public interface ITicketApiService
    {
        /// <summary>
        /// Purchases tickets for the specified showtime and seats.
        /// </summary>
        /// <param name="request">Ticket purchase request.</param>
        /// <param name="token">Optional bearer token used for authenticated purchases.</param>
        Task BuyTicketAsync(BuyTicketDTO request, string? token);

        /// <summary>
        /// Reserves tickets for the specified showtime and seats.
        /// </summary>
        /// <param name="request">Ticket reservation request.</param>
        /// <param name="token">Optional bearer token used for authenticated reservations.</param>
        Task ReserveTicketsAsync(ReserveTicketDTO request, string? token);
    }
}
