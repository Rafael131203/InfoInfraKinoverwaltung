using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinoAppShared.DTOs.Kinosaal
{
    public class CreateSitzreiheDTO
    {
   
        public int Kategorie {  get; set; }
        public string Bezeichnung { get; set; }
        public long KinosaalId { get; set; }
        public CreateSitzreiheDTO(int kategorie, string bezeichnung, long kinosaalId)
        {
            Kategorie = kategorie;
            Bezeichnung = bezeichnung;
            KinosaalId = kinosaalId;
        }
    }
}
