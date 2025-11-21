using KinoAppDB.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinoAppDB.Repository
{
    public class VorstellungRepository : Repository<VorstellungEntity>, IVorstellungRepository
    {
        public VorstellungRepository(KinoAppDbContextScope scope) : base(scope) { }
    }
}
