using KinoAppDB.Entities;

namespace KinoAppDB.Repository
{
    /// <summary>
    /// Repository contract for ticket persistence operations including common ticket-centric queries.
    /// </summary>
    /// <remarks>
    /// These query helpers exist to keep ticket-related read patterns consistent and to avoid duplicating
    /// filtering logic across services.
    /// </remarks>
    public interface ITicketRepository : IRepository<TicketEntity>
    {
        /// <summary>
        /// Returns all tickets associated with the specified user.
        /// </summary>
        /// <param name="userId">User identifier.</param>
        /// <param name="ct">Cancellation token.</param>
        Task<List<TicketEntity>> GetTicketsByUserIdAsync(long userId, CancellationToken ct = default);

        /// <summary>
        /// Returns all tickets for the specified showing.
        /// </summary>
        /// <param name="vorstellungId">Showing identifier.</param>
        /// <param name="ct">Cancellation token.</param>
        Task<List<TicketEntity>> GetTicketsByVorstellungIdAsync(long vorstellungId, CancellationToken ct = default);

        /// <summary>
        /// Returns the seat IDs that are currently occupied (reserved or booked) for the specified showing.
        /// </summary>
        /// <param name="vorstellungId">Showing identifier.</param>
        /// <param name="ct">Cancellation token.</param>
        Task<List<long>> GetBookedSeatIdsAsync(long vorstellungId, CancellationToken ct = default);

        /// <summary>
        /// Returns the count of free seats for the specified showing.
        /// </summary>
        /// <param name="vorstellungId">Showing identifier.</param>
        /// <param name="ct">Cancellation token.</param>
        Task<int> GetFreeSeatCountAsync(long vorstellungId, CancellationToken ct = default);
    }
}
