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
        public string MovieTitle { get; set; } = string.Empty;
        public int SoldTickets { get; set; }

        /// <summary>
        /// Der Gesamtpreis für die verkauften Tickets.
        /// Wird als BsonType.Decimal128 in MongoDB gespeichert, um finanzielle Genauigkeit zu gewährleisten.
        /// </summary>
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal TotalPrice { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    }
}
