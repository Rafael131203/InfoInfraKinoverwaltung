using KinoAppShared.Enums;

namespace KinoAppDB.Entities
{
    public class PreisZuKategorieEntity
    {
        public long Id { get; set; }
        public SitzreihenKategorie Kategorie { get; set; }
        public decimal Preis {  get; set; }
    }
}
