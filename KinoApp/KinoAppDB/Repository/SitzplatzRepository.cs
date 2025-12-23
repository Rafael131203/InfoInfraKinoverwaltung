using KinoAppDB.Entities;

namespace KinoAppDB.Repository
{
    public class SitzplatzRepository : Repository<SitzplatzEntity>, ISitzplatzRepository
    {
        public SitzplatzRepository(KinoAppDbContextScope scope) : base(scope) { }
    }
}
