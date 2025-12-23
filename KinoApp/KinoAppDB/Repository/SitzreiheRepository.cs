using KinoAppDB.Entities;

namespace KinoAppDB.Repository
{
    /// <summary>
    /// EF-backed repository implementation for <see cref="SitzreiheEntity"/>.
    /// </summary>
    public class SitzreiheRepository : Repository<SitzreiheEntity>, ISitzreiheRepository
    {
        /// <summary>
        /// Creates a new <see cref="SitzreiheRepository"/>.
        /// </summary>
        /// <param name="scope">Database context scope used to access the current <see cref="KinoAppDbContext"/>.</param>
        public SitzreiheRepository(KinoAppDbContextScope scope) : base(scope) { }
    }
}
