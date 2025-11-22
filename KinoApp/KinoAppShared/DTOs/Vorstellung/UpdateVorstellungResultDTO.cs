using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinoAppShared.DTOs.Vorstellung
{
    public class UpdateVorstellungResultDTO
    {
        public VorstellungDTO VorstellungAlt { get; set; }
        public VorstellungDTO VorstellungNeu { get; set; }
    }
}
