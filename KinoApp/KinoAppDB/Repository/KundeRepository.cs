using KinoAppCore.Abstractions;
using KinoAppCore.Entities;
using Microsoft.EntityFrameworkCore;

namespace KinoAppDB.Repository;

public sealed class KundeRepository : Repository<Kunde>, IKundeRepository
{
    private readonly KinoAppDbContext _db;

    public KundeRepository(KinoAppDbContext db) : base(db) { }

    public Task<Kunde?> FindByEmailAsync(string email)
        => _db.Kunden.FirstOrDefaultAsync(k => k.Email == email);

}
