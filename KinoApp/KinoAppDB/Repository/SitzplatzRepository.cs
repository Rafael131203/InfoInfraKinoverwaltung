using KinoAppDB.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinoAppDB.Repository
{
    public class SitzplatzRepository : Repository<SitzplatzEntity>, ISitzplatzRepository
    {
        public SitzplatzRepository(KinoAppDbContextScope scope) : base(scope) { }
    }
}
