using KinoAppDB.Entities;

namespace KinoAppDB.Repository
{
    /// <summary>
    /// EF-backed repository implementation for <see cref="KinosaalEntity"/>.
    /// </summary>
    public class KinosaalRepository : Repository<KinosaalEntity>, IKinosaalRepository
    {
        /// <summary>
        /// Creates a new <see cref="KinosaalRepository"/>.
        /// </summary>
        /// <param name="scope">Database context scope used to access the current <c>DbContext</c>.</param>
        public KinosaalRepository(KinoAppDbContextScope scope) : base(scope)
        {
        }
    }
}
