using System.Text.Json.Serialization;

namespace KinoAppDB.Entities
{
    public class SitzplatzEntity
    {
        public long Id { get; set; }
        public int Nummer { get; set; }
        public decimal Preis { get; set; }
        public long SitzreiheId { get; set; }
        [JsonIgnore]
        public SitzreiheEntity? Sitzreihe { get; set; }
    }
}
