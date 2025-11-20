using KinoAppShared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinoAppDB.Entities
{
    public class PreisZuKategorieEntity
    {
        public long Id { get; set; }
        public SitzreihenKategorie Kategorie { get; set; }
        public decimal Preis {  get; set; }
    }
}
