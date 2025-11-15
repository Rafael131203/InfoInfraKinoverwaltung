using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinoAppShared.DTOs
{
    public class WarenkorbDTO
    {
        public int Id { get; set; }
        public decimal Gesamtpreis { get; set; }
        public int Zahlungsmittel { get; set; }
    }
}
