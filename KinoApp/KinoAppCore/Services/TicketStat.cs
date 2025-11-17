using KinoAppCore.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinoAppCore.Services
{
    // KinoAppShared.Messaging/TicketStat.cs

    public class TicketStat
    {
        public string filmTitel { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.Decimal128)]
        public decimal preis { get; set; }

        public string genre { get; set; }

        public Platzkategorie platzkategorie { get; set; }

        public DateTime vorstellungsbeginn { get; set; }

        public Zahlungsmittel zahlungsmittel    { get; set; }
    }
}
