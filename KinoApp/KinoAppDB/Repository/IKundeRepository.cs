using KinoAppDB.Entities;

namespace KinoAppDB.Repository;

public interface IKundeRepository : IRepository<KundeEntity>
{
    Task<KundeEntity?> FindByEmailAsync(string email, CancellationToken ct = default);
}
