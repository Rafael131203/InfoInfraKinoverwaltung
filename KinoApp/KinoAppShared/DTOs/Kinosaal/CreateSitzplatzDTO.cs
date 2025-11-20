using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinoAppShared.DTOs.Kinosaal
{
    public class CreateSitzplatzDTO
    {
        public Boolean Gebucht {  get; set; }
        public int Nummer {  get; set; }
        public decimal Preis {  get; set; }
        public long SitzreihenId { get; set; }
    }
}
