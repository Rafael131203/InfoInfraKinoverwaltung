using KinoAppDB.Entities;
using Microsoft.EntityFrameworkCore;

namespace KinoAppDB.Repository;

public sealed class KundeRepository : Repository<KundeEntity>, IKundeRepository
{
    public KundeRepository(KinoAppDbContextScope scope) : base(scope) { }

    public Task<KundeEntity?> FindByEmailAsync(string email, CancellationToken ct = default)
        => Query().FirstOrDefaultAsync(k => k.Email == email, ct);
}
