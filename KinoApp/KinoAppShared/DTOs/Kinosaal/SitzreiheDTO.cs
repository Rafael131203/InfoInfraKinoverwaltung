using KinoAppShared.Enums;

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
