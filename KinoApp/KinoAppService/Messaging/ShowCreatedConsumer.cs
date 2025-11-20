using System;
using System.Threading.Tasks;
using KinoAppShared.Messaging;
using MassTransit;

namespace KinoAppService.Messaging
{
    public sealed class ShowCreatedConsumer : IConsumer<ShowCreated>
    {
        public Task Consume(ConsumeContext<ShowCreated> ctx)
        {
            var e = ctx.Message;

            Console.WriteLine($"[ShowCreatedConsumer] Neue Vorstellung: Film {e.FilmId} in Saal {e.KinosaalId} um {e.StartTimeUtc}");

            return Task.CompletedTask;
        }
    }
}
