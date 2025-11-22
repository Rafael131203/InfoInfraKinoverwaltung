using KinoAppShared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static KinoAppShared.Enums.VorstellungStatusEnum;

namespace KinoAppDB.Entities
{
    public class VorstellungEntity
    {
        public long Id { get; set; }
        public DateTime Datum { get; set; }
        public required VorstellungStatusEnum Status { get; set; }
        public string? FilmId { get; set; }
        public FilmEntity Film { get; set; }
        public long? KinosaalId { get; set; }
        public  KinosaalEntity Kinosaal { get; set; }
    }
}
