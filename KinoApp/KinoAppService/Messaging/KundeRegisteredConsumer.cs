using System;
using KinoAppCore.Documents;
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

    public async Task Consume(ConsumeContext<KundeRegistered> ctx)
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

        // upsert, falls mehrmals dasselbe Event kommt
        try
        {
            // Wir warten explizit auf das Speichern
            await _col.ReplaceOneAsync(
                f => f.KundeId == e.KundeId,
                doc,
                new ReplaceOptions { IsUpsert = true }
            );

            
            // DEBUG Console.WriteLine($" ERFOLGREICH: Kunde {e.KundeId} in MongoDB gespeichert!");
        }
        catch (Exception ex)
        {
            
            Console.WriteLine($" MONGO-FEHLER: {ex.Message}");
            // Fehler weiterwerfen, damit MassTransit es merkt und ggf. wiederholt
            throw;
        }
    }
}