using System;
using System.Threading.Tasks;
using KinoAppShared.Messaging;
using MassTransit;
using MongoDB.Driver;

namespace KinoAppService.Messaging;

public sealed class KundeRegisteredConsumer : IConsumer<KundeRegistered>
{
    private readonly IMongoCollection<KundeRegistrationProjection> _col;

    public KundeRegisteredConsumer(IMongoClient client)
    {
        _col = client.GetDatabase("stats")
                     .GetCollection<KundeRegistrationProjection>("registered_customers");
    }

    public Task Consume(ConsumeContext<KundeRegistered> ctx)
    {
        var e = ctx.Message;

        var doc = new KundeRegistrationProjection
        {
            KundeId = e.KundeId,
            Email = e.Email,
            Vorname = e.Vorname,
            Nachname = e.Nachname,
            RegisteredAtUtc = e.RegisteredAtUtc
        };

        // upsert, falls du mehrmals dasselbe Event bekommst
        return _col.ReplaceOneAsync(
            f => f.KundeId == e.KundeId,
            doc,
            new ReplaceOptions { IsUpsert = true });
    }

    public sealed class KundeRegistrationProjection
    {
        public long KundeId { get; set; }
        public string Email { get; set; } = null!;
        public string Vorname { get; set; } = null!;
        public string Nachname { get; set; } = null!;
        public DateTime RegisteredAtUtc { get; set; }
    }
}