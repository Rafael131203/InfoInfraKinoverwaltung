using KinoAppShared.Enums;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static KinoAppShared.Enums.VorstellungStatusEnum;

namespace KinoAppShared.DTOs.Vorstellung
{
    public class CreateVorstellungDTO
    {
        public DateTime Datum { get; set; }
        public VorstellungStatusEnum Status { get; set; }
        public string FilmId { get; set; }
        public long KinosaalId {  get; set; }
    }
}
