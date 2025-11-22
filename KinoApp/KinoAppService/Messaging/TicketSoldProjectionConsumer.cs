using KinoAppShared.Messaging;
using MassTransit;
using MongoDB.Driver;

public sealed class TicketSoldProjectionConsumer : IConsumer<TicketSold>
{
    private readonly IMongoCollection<DailyShowRevenue> _col;

    public TicketSoldProjectionConsumer(IMongoClient client)
    {
        _col = client.GetDatabase("stats").GetCollection<DailyShowRevenue>("daily_revenue");
    }

    public async Task Consume(ConsumeContext<TicketSold> ctx)
    {
        var e = ctx.Message;

        // Wir schneiden die Uhrzeit ab, um nur das Datum zu haben (00:00:00)
        var verkaufsTag = e.SoldAtUtc.Date;

        // Der Filter sucht jetzt nach: GLEICHE Show UND GLEICHER Tag
        var filter = Builders<DailyShowRevenue>.Filter.And(
            Builders<DailyShowRevenue>.Filter.Eq(x => x.ShowId, e.ShowId),
            Builders<DailyShowRevenue>.Filter.Eq(x => x.Day, verkaufsTag)
        );

        var update = Builders<DailyShowRevenue>.Update
            .Inc(x => x.SoldTickets, e.Quantity)
            .Inc(x => x.Revenue, e.TotalPrice)
            .Set(x => x.LastUpdatedUtc, DateTime.UtcNow);

        // Upsert: Wenn es für HEUTE noch keinen Eintrag gibt -> Erstellen!
        await _col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
    }

    // Angepasste Klasse
    public class DailyShowRevenue
    {
        public object Id { get; set; } // Mongo ObjectId
        public long ShowId { get; set; }
        public DateTime Day { get; set; } 
        public int SoldTickets { get; set; }
        public decimal Revenue { get; set; }
        public DateTime LastUpdatedUtc { get; set; }
    }
}