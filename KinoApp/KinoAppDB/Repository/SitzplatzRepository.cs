using KinoAppDB.Entities;

namespace KinoAppDB.Repository
{
    /// <summary>
    /// EF-backed repository implementation for <see cref="SitzplatzEntity"/>.
    /// </summary>
    public class SitzplatzRepository : Repository<SitzplatzEntity>, ISitzplatzRepository
    {
        /// <summary>
        /// Creates a new <see cref="SitzplatzRepository"/>.
        /// </summary>
        /// <param name="scope">Database context scope used to access the current <see cref="KinoAppDbContext"/>.</param>
        public SitzplatzRepository(KinoAppDbContextScope scope) : base(scope) { }
    }
}
