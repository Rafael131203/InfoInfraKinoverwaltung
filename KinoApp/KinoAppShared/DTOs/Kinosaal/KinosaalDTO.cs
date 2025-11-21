using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinoAppShared.DTOs.Kinosaal
{
    public class KinosaalDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public List<SitzreiheDTO> Sitzreihen { get; set; }
    }

}
