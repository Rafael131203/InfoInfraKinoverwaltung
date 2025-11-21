using KinoAppShared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinoAppShared.DTOs.Kinosaal
{
    public class SitzreiheDTO
    {
        public long Id { get; set; }
        public string Bezeichnung { get; set; }
        public SitzreihenKategorie Kategorie { get; set; }
        public List<SitzplatzDTO> Sitzplätze { get; set; }
    }
}
