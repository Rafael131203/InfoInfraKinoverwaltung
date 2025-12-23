using KinoAppShared.Enums;

namespace KinoAppShared.DTOs.Kinosaal
{
        public class SetPreisDTO
        {
            public SitzreihenKategorie Kategorie { get; set; }
            public decimal Preis { get; set; }
        }

}
