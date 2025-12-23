using KinoAppDB.Entities;
using KinoAppShared.Enums;
using Microsoft.EntityFrameworkCore;

namespace KinoAppDB.Repository
{
    /// <summary>
    /// EF-backed repository implementation for <see cref="TicketEntity"/> including ticket-specific query helpers.
    /// </summary>
    public sealed class TicketRepository : Repository<TicketEntity>, ITicketRepository
    {
        /// <summary>
        /// Creates a new <see cref="TicketRepository"/>.
        /// </summary>
        /// <param name="scope">Database context scope used to access the current <see cref="KinoAppDbContext"/>.</param>
        public TicketRepository(KinoAppDbContextScope scope) : base(scope)
        {
        }

        /// <inheritdoc />
        public Task<List<TicketEntity>> GetTicketsByUserIdAsync(long userId, CancellationToken ct = default)
        {
            return Query()
                .Where(t => t.UserId == userId)
                .ToListAsync(ct);
        }

        /// <inheritdoc />
        public Task<List<TicketEntity>> GetTicketsByVorstellungIdAsync(long vorstellungId, CancellationToken ct = default)
        {
            return Query()
                .Where(t => t.VorstellungId == vorstellungId)
                .ToListAsync(ct);
        }

        /// <inheritdoc />
        public Task<List<long>> GetBookedSeatIdsAsync(long vorstellungId, CancellationToken ct = default)
        {
            int booked = (int)TicketStatus.Booked;
            int reserved = (int)TicketStatus.Reserved;

            return Query()
                .Where(t => t.VorstellungId == vorstellungId &&
                            (t.Status == booked || t.Status == reserved))
                .Select(t => t.SitzplatzId)
                .ToListAsync(ct);
        }

        /// <inheritdoc />
        public Task<int> GetFreeSeatCountAsync(long vorstellungId, CancellationToken ct = default)
        {
            int free = (int)TicketStatus.Free;

            return Query()
                .Where(t => t.VorstellungId == vorstellungId && t.Status == free)
                .CountAsync(ct);
        }
    }
}
