using KinoAppDB.Entities;

namespace KinoAppDB.Repository
{
    /// <summary>
    /// EF-backed repository implementation for <see cref="PreisZuKategorieEntity"/>.
    /// </summary>
    public class PreisZuKategorieRepository : Repository<PreisZuKategorieEntity>, IPreisZuKategorieRepository
    {
        /// <summary>
        /// Creates a new <see cref="PreisZuKategorieRepository"/>.
        /// </summary>
        /// <param name="scope">Database context scope used to access the current <see cref="KinoAppDbContext"/>.</param>
        public PreisZuKategorieRepository(KinoAppDbContextScope scope) : base(scope) { }
    }
}
