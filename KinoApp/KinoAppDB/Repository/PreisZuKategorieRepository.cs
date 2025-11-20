using KinoAppDB.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinoAppDB.Repository
{
    public class PreisZuKategorieRepository : Repository<PreisZuKategorieEntity>,IPreisZuKategorieRepository
    {
        public PreisZuKategorieRepository(KinoAppDbContextScope scope) : base(scope) { }
    }
}
