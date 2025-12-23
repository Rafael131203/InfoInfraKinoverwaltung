using KinoAppDB.Entities;

namespace KinoAppDB.Repository
{
    public class SitzreiheRepository : Repository<SitzreiheEntity>, ISitzreiheRepository
    {
        public SitzreiheRepository(KinoAppDbContextScope scope) : base(scope) { }
    }
}
