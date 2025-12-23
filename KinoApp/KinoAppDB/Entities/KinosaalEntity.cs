namespace KinoAppDB.Entities
{
    public class KinosaalEntity
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public ICollection<SitzreiheEntity> Sitzreihen { get; set; } = new List<SitzreiheEntity>();
        public ICollection<VorstellungEntity> Vorstellungen { get; set; } = new List<VorstellungEntity>();
    }
}
