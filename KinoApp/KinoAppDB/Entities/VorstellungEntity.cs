using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinoAppDB.Entities
{
    public class VorstellungEntity
    {
        public long Id { get; set; }
        public DateTime Datum { get; set; }
        public long? FilmId { get; set; }
        public FilmEntity Film { get; set; }
    }
}
