using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace KinoAppCore.Documents
{
    /// <summary>
    /// Represents the aggregated revenue data for a single show on a specific day.
    /// </summary>
    /// <remarks>
    /// This document is stored in MongoDB and is intended for reporting and analytics
    /// use cases where daily ticket sales and revenue need to be queried efficiently.
    /// </remarks>
    public class DailyShowRevenue
    {
        /// <summary>
        /// The MongoDB document identifier.
        /// </summary>
        [BsonId]
        public ObjectId Id { get; set; }

        /// <summary>
        /// The unique identifier of the show this revenue entry belongs to.
        /// </summary>
        public long ShowId { get; set; }

        /// <summary>
        /// The calendar day the revenue data applies to.
        /// </summary>
        /// <remarks>
        /// This value represents the logical business day and is independent of the
        /// document's last update timestamp.
        /// </remarks>
        public DateTime Day { get; set; }

        /// <summary>
        /// The total number of tickets sold for the show on the specified day.
        /// </summary>
        public int SoldTickets { get; set; }

        /// <summary>
        /// The total revenue generated from ticket sales for the show on the specified day.
        /// </summary>
        public decimal Revenue { get; set; }

        /// <summary>
        /// The UTC timestamp indicating when this document was last updated.
        /// </summary>
        public DateTime LastUpdatedUtc { get; set; }
    }
}
