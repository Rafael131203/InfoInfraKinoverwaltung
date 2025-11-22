using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinoAppShared.DTOs.Vorstellung
{
    public class UpdateVorstellungDTO
    {   
        public long Id { get; set; }
        public string? FilmId { get; set; }
        public DateTime? Datum { get; set; }
    }
}
