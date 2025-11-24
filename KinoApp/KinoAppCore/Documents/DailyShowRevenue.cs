using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace KinoAppCore.Documents
{
    public class DailyShowRevenue
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public long ShowId { get; set; }
        public DateTime Day { get; set; }
        public int SoldTickets { get; set; }
        public decimal Revenue { get; set; }
        public DateTime LastUpdatedUtc { get; set; }
    }
}