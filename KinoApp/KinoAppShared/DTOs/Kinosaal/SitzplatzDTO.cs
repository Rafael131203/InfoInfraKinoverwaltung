using KinoAppShared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinoAppShared.DTOs.Kinosaal
{
    public class SitzplatzDTO
    {
        public long Id { get; set; }
        public int Nummer { get; set; }
        public decimal Preis { get; set; }
        public Boolean Gebucht { get; set; }
        public TicketStatus Status { get; set; }
    }
}
