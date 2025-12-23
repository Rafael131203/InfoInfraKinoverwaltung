using KinoAppDB.Entities;

namespace KinoAppDB.Repository
{
    public class PreisZuKategorieRepository : Repository<PreisZuKategorieEntity>,IPreisZuKategorieRepository
    {
        public PreisZuKategorieRepository(KinoAppDbContextScope scope) : base(scope) { }
    }
}
