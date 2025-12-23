using KinoAppDB.Entities;

namespace KinoAppDB.Repository
{
    /// <summary>
    /// EF-backed repository implementation for <see cref="VorstellungEntity"/>.
    /// </summary>
    public class VorstellungRepository : Repository<VorstellungEntity>, IVorstellungRepository
    {
        /// <summary>
        /// Creates a new <see cref="VorstellungRepository"/>.
        /// </summary>
        /// <param name="scope">Database context scope used to access the current <see cref="KinoAppDbContext"/>.</param>
        public VorstellungRepository(KinoAppDbContextScope scope) : base(scope) { }
    }
}
