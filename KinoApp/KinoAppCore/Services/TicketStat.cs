using KinoAppCore.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KinoAppCore.Services
{
    public class TicketStat
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;
        public string filmTitel { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.Decimal128)]
        public decimal preis { get; set; }

        public string genre { get; set; }

        public Platzkategorie platzkategorie { get; set; }

        public DateTime vorstellungsbeginn { get; set; }

        public Zahlungsmittel zahlungsmittel    { get; set; }
    }
}
