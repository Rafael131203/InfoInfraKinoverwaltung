using KinoAppDB.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
            return Query()
                .Where(t => t.VorstellungId == vorstellungId)
                .Select(t => t.SitzplatzId) // Da SitzplatzId 'long' ist, kommt hier 'long' raus
                .ToListAsync(ct);
        }
    }
}