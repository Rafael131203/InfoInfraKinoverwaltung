using KinoAppShared.DTOs;
using KinoAppShared.DTOs.Ticket;

namespace KinoAppCore.Services
{
    /// <summary>
    /// Provides ticket purchasing, reservation, cancellation, and query operations.
    /// </summary>
    /// <remarks>
    /// This service coordinates ticket status transitions (free, reserved, booked) for a given showing (Vorstellung)
    /// and exposes user-centric queries for retrieving purchased or reserved tickets.
    /// </remarks>
    public interface ITicketService
    {
        /// <summary>
        /// Purchases one or more tickets for the specified request.
        /// </summary>
        /// <param name="request">Purchase request containing the showing and requested seat IDs.</param>
        /// <param name="userId">Optional user identifier for associating tickets with a user.</param>
        /// <returns>A list of ticket DTOs representing the created or updated tickets.</returns>
        Task<List<BuyTicketDTO>> BuyTicketsAsync(BuyTicketDTO request, long? userId);

        /// <summary>
        /// Cancels tickets by their identifiers.
        /// </summary>
        /// <param name="ticketIds">Ticket identifiers to cancel.</param>
        Task CancelTicketsAsync(List<long> ticketIds);

        /// <summary>
        /// Returns all tickets associated with the specified user.
        /// </summary>
        /// <param name="userId">User identifier.</param>
        Task<List<BuyTicketDTO>> GetTicketsByUserIdAsync(long userId);

        /// <summary>
        /// Retrieves a single ticket by its identifier.
        /// </summary>
        /// <param name="ticketId">Ticket identifier.</param>
        Task<BuyTicketDTO> GetTicketAsync(long ticketId);

        /// <summary>
        /// Returns all tickets.
        /// </summary>
        Task<List<BuyTicketDTO>> GetAllAsync();

        /// <summary>
        /// Creates the initial ticket records for a showing, typically one per seat in the associated auditorium.
        /// </summary>
        /// <param name="vorstellungId">Showing identifier.</param>
        /// <param name="kinosaalId">Optional auditorium identifier if it cannot be inferred.</param>
        /// <param name="ct">Cancellation token.</param>
        Task CreateTicketsForVorstellungAsync(long vorstellungId, long? kinosaalId, CancellationToken ct);

        /// <summary>
        /// Returns the number of seats that are currently available for the specified showing.
        /// </summary>
        /// <param name="vorstellungId">Showing identifier.</param>
        /// <param name="ct">Cancellation token.</param>
        Task<int> GetFreeSeatCountAsync(long vorstellungId, CancellationToken ct);

        /// <summary>
        /// Updates the status of one or more tickets.
        /// </summary>
        /// <param name="dto">Status update request.</param>
        /// <param name="ct">Cancellation token.</param>
        Task UpdateTicketStatusAsync(UpdateTicketStatusDTO dto, CancellationToken ct);

        /// <summary>
        /// Reserves one or more tickets for the specified user.
        /// </summary>
        /// <param name="request">Reservation request.</param>
        /// <param name="userId">Optional user identifier for associating the reservation with a user.</param>
        /// <param name="ct">Cancellation token.</param>
        Task ReserveTicketsAsync(ReserveTicketDTO request, long? userId, CancellationToken ct);
    }
}
