using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinoAppShared.DTOs.Kinosaal
{
    public class CreateSitzplatzDTO
    {
        public Boolean Gebucht {  get; set; }
        public int Nummer {  get; set; }
        public decimal Price { get; set; }

        public long SitzreiheId { get; set; }

        public CreateSitzplatzDTO(Boolean gebucht, int nummer, decimal price, long sitzreiheId)
        {
            Gebucht = gebucht;
            Nummer = nummer;
            Price = price;
            SitzreiheId = sitzreiheId;
        }
    }
}
