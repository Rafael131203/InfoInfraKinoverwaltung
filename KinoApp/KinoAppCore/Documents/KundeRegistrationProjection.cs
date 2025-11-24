using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace KinoAppCore.Documents
{
    // Diese Klasse repräsentiert ein Dokument in der Mongo-Collection "registered_customers"
    public class KundeRegistrationProjection
    {
        [BsonId] // Mongo braucht eine interne ID
        public ObjectId Id { get; set; }

        public long KundeId { get; set; } // Deine ID aus Postgres
        public string Email { get; set; } = null!;
        public string Vorname { get; set; } = null!;
        public string Nachname { get; set; } = null!;
        public DateTime RegisteredAtUtc { get; set; }
    }
}