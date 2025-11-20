using KinoAppCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinoAppShared.DTOs.Kinosaal
{
        public class SetPreisDTO
        {
            public SitzreihenKategorie Kategorie { get; set; }
            public decimal Preis { get; set; }
        }

}
