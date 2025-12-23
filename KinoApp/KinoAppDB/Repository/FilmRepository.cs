using KinoAppDB.Entities;

namespace KinoAppDB.Repository
{
    /// <summary>
    /// EF-backed repository implementation for <see cref="FilmEntity"/>.
    /// </summary>
    public class FilmRepository : Repository<FilmEntity>, IFilmRepository
    {
        /// <summary>
        /// Creates a new <see cref="FilmRepository"/>.
        /// </summary>
        /// <param name="scope">Database context scope used to access the current <c>DbContext</c>.</param>
        public FilmRepository(KinoAppDbContextScope scope) : base(scope)
        {
        }
    }
}
