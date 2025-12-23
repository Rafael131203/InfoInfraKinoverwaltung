using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace KinoAppCore.Documents
{
    /// <summary>
    /// Represents a projection of a registered customer stored in MongoDB.
    /// </summary>
    /// <remarks>
    /// This document acts as a read-optimized representation of customer registration data,
    /// typically originating from the relational database and synchronized via events.
    /// </remarks>
    public class KundeRegistrationProjection
    {
        /// <summary>
        /// The MongoDB document identifier.
        /// </summary>
        [BsonId]
        public ObjectId Id { get; set; }

        /// <summary>
        /// The unique identifier of the customer from the relational database.
        /// </summary>
        public long KundeId { get; set; }

        /// <summary>
        /// The email address of the registered customer.
        /// </summary>
        public string Email { get; set; } = null!;

        /// <summary>
        /// The customer's first name.
        /// </summary>
        public string Vorname { get; set; } = null!;

        /// <summary>
        /// The customer's last name.
        /// </summary>
        public string Nachname { get; set; } = null!;

        /// <summary>
        /// The UTC timestamp indicating when the customer was registered.
        /// </summary>
        public DateTime RegisteredAtUtc { get; set; }
    }
}
