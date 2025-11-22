using KinoAppShared.Messaging;
using MassTransit;
using MongoDB.Driver;

public sealed class TicketSoldProjectionConsumer : IConsumer<TicketSold>, IConsumer<TicketCancelled> // Wichtig: Beide Interfaces!
{
    private readonly IMongoCollection<DailyShowRevenue> _col;

    public TicketSoldProjectionConsumer(IMongoClient client)
    {
        _col = client.GetDatabase("stats").GetCollection<DailyShowRevenue>("daily_revenue");
    }

    // KAUF
    public async Task Consume(ConsumeContext<TicketSold> ctx)
    {
        var e = ctx.Message;
        var verkaufsTag = e.SoldAtUtc.Date;

        var filter = Builders<DailyShowRevenue>.Filter.And(
            Builders<DailyShowRevenue>.Filter.Eq(x => x.ShowId, e.ShowId),
            Builders<DailyShowRevenue>.Filter.Eq(x => x.Day, verkaufsTag)
        );

        var update = Builders<DailyShowRevenue>.Update
            .Inc(x => x.SoldTickets, e.Quantity)
            .Inc(x => x.Revenue, e.TotalPrice)
            .Set(x => x.LastUpdatedUtc, DateTime.UtcNow);

        await _col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
    }

    // STORNO
    public async Task Consume(ConsumeContext<TicketCancelled> ctx)
    {
        var e = ctx.Message;

        var stornoTag = e.CancelledAtUtc.Date;

        var filter = Builders<DailyShowRevenue>.Filter.And(
            Builders<DailyShowRevenue>.Filter.Eq(x => x.ShowId, e.ShowId),
            Builders<DailyShowRevenue>.Filter.Eq(x => x.Day, stornoTag) //
        );

        var update = Builders<DailyShowRevenue>.Update
            .Inc(x => x.SoldTickets, -1)
            .Inc(x => x.Revenue, -e.AmountToRefund)
            .Set(x => x.LastUpdatedUtc, DateTime.UtcNow);

        // Upsert hier auch true lassen (falls Storno am selben Tag wie Kauf passiert und noch nichts geschrieben war - unwahrscheinlich, aber sicher)
        await _col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });

        Console.WriteLine($"STORNO: Ticket {e.TicketId} storniert. Umsatz korrigiert.");
    }

    public class DailyShowRevenue
    {
        public object Id { get; set; }
        public long ShowId { get; set; }
        public DateTime Day { get; set; }
        public int SoldTickets { get; set; }
        public decimal Revenue { get; set; }
        public DateTime LastUpdatedUtc { get; set; }
    }
}