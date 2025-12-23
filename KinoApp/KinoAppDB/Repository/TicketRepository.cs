using KinoAppDB.Entities;
using KinoAppShared.Enums;
using Microsoft.EntityFrameworkCore;

namespace KinoAppDB.Repository
{
    public sealed class TicketRepository : Repository<TicketEntity>, ITicketRepository
    {
        public TicketRepository(KinoAppDbContextScope scope) : base(scope)
        {
        }

        public Task<List<TicketEntity>> GetTicketsByUserIdAsync(long userId, CancellationToken ct = default)
        {
            return Query()
                .Where(t => t.UserId == userId)
                .ToListAsync(ct);
        }

        public Task<List<TicketEntity>> GetTicketsByVorstellungIdAsync(long vorstellungId, CancellationToken ct = default)
        {
            return Query()
                .Where(t => t.VorstellungId == vorstellungId)
                .ToListAsync(ct);
        }

        // KORREKTUR: Rückgabetyp ist jetzt Task<List<long>>
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

        public Task<int> GetFreeSeatCountAsync(long vorstellungId, CancellationToken ct = default)
        {
            int free = (int)TicketStatus.Free;

            return Query()
                .Where(t => t.VorstellungId == vorstellungId && t.Status == free)
                .CountAsync(ct);
        }
    }
}