using KinoAppDB.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinoAppDB.Repository
{
    public class SitzreiheRepository : Repository<SitzreiheEntity>, ISitzreiheRepository
    {
        public SitzreiheRepository(KinoAppDbContextScope scope) : base(scope) { }
    }
}
