using KinoAppShared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinoAppShared.DTOs.Kinosaal
{
    public class ChangeKategorieSitzreiheDTO
    {
        public long Id { get; set; }
        public SitzreihenKategorie Kategorie{ get; set; }
    }
}
