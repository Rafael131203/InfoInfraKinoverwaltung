using KinoAppDB.Entities;

namespace KinoAppDB.Repository
{
    /// <summary>
    /// Repository contract for film persistence operations.
    /// </summary>
    public interface IFilmRepository : IRepository<FilmEntity>
    {
    }
}
