using KinoAppCore.Abstractions;
using KinoAppCore.Entities;
using Microsoft.EntityFrameworkCore;

namespace KinoAppDB.Repository;

public sealed class KundeRepository : Repository<Kunde>, IKundeRepository
{
    public KundeRepository(KinoAppDbContextScope scope) : base(scope) { }

    public Task<Kunde?> FindByEmailAsync(string email, CancellationToken ct = default)
        => Query().FirstOrDefaultAsync(k => k.Email == email, ct);
}
