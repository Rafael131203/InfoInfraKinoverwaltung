using System;
using System.Threading.Tasks;
using KinoAppShared.Messaging;
using MassTransit;
using MongoDB.Driver;

namespace KinoAppService.Messaging;

public sealed class TicketSoldProjectionConsumer : IConsumer<TicketSold>
{
    private readonly IMongoCollection<ShowRevenue> _col;

    public TicketSoldProjectionConsumer(IMongoClient client)
    {
        _col = client.GetDatabase("stats")
                     .GetCollection<ShowRevenue>("revenue");
    }

    public Task Consume(ConsumeContext<TicketSold> ctx)
    {
        var e = ctx.Message;

        var update = Builders<ShowRevenue>.Update
            .Inc(x => x.SoldTickets, e.Quantity)
            .Inc(x => x.Revenue, e.TotalPrice)
            .Set(x => x.UpdatedUtc, DateTime.UtcNow);

        return _col.UpdateOneAsync(f => f.ShowId == e.ShowId,
                                   update,
                                   new UpdateOptions { IsUpsert = true });
    }

    public sealed class ShowRevenue
    {
        public long ShowId { get; set; }   
        public int SoldTickets { get; set; }
        public decimal Revenue { get; set; }
        public DateTime UpdatedUtc { get; set; }
    }

}
