using KinoAppDB.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinoAppDB.Repository
{
    public class KinosaalRepository : Repository<KinosaalEntity>, IKinosaalRepository
    {
        public KinosaalRepository(KinoAppDbContextScope scope) : base(scope) { }
    }
}
