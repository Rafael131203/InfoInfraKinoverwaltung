using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinoAppDB.Entities
{
    public class SitzreiheEntity
    {
        public long Id { get; set; }
        public int Kategorie {  get; set; }
        public string Bezeichnung { get; set; }
        public long? KinosaalId  { get; set; }
        public KinosaalEntity? Kinosaal { get; set; }
        public ICollection<SitzplatzEntity> Sitzplätze { get; set; } = new List<SitzplatzEntity>();
    }
}
