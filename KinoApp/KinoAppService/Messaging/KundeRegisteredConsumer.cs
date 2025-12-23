using System;
using System.Threading.Tasks;
using KinoAppCore.Documents;
using KinoAppShared.Messaging;
using MassTransit;
using MongoDB.Driver;

namespace KinoAppService.Messaging
{
    /// <summary>
    /// MassTransit consumer that projects <see cref="KundeRegistered"/> events into MongoDB.
    /// </summary>
    /// <remarks>
    /// Uses an upsert operation to make the projection idempotent in case the event is delivered more than once.
    /// </remarks>
    public sealed class KundeRegisteredConsumer : IConsumer<KundeRegistered>
    {
        private readonly IMongoCollection<KundeRegistrationProjection> _col;

        /// <summary>
        /// Creates a new <see cref="KundeRegisteredConsumer"/>.
        /// </summary>
        /// <param name="client">MongoDB client used to access the projections database.</param>
        public KundeRegisteredConsumer(IMongoClient client)
        {
            _col = client.GetDatabase("stats")
                         .GetCollection<KundeRegistrationProjection>("registered_customers");
        }

        /// <inheritdoc />
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

            try
            {
                await _col.ReplaceOneAsync(
                    f => f.KundeId == e.KundeId,
                    doc,
                    new ReplaceOptions { IsUpsert = true }
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($" MONGO-FEHLER: {ex.Message}");
                throw;
            }
        }
    }
}
