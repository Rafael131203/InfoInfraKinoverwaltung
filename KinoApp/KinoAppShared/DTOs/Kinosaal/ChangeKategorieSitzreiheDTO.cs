using KinoAppShared.Enums;

namespace KinoAppShared.DTOs.Kinosaal
{
    public class ChangeKategorieSitzreiheDTO
    {
        public long Id { get; set; }
        public SitzreihenKategorie Kategorie{ get; set; }
    }
}
