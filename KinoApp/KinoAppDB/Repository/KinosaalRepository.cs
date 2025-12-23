using KinoAppDB.Entities;

namespace KinoAppDB.Repository
{
    public class KinosaalRepository : Repository<KinosaalEntity>, IKinosaalRepository
    {
        public KinosaalRepository(KinoAppDbContextScope scope) : base(scope) { }
    }
}
