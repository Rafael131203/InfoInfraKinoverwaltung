using KinoAppDB.Entities;

namespace KinoAppDB.Repository
{
    public class FilmRepository : Repository<FilmEntity>, IFilmRepository
    {
        public FilmRepository(KinoAppDbContextScope scope) : base(scope)
        {
        }
    }
}
