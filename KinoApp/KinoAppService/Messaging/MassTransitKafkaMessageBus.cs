using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using KinoAppCore.Abstractions;

namespace KinoAppService.Messaging;

public sealed class MassTransitKafkaMessageBus : IMessageBus
{
    private readonly IServiceProvider _services;
    public MassTransitKafkaMessageBus(IServiceProvider services) => _services = services;

    public Task PublishAsync<T>(T message, CancellationToken ct = default) where T : class
    {
        var producer = _services.GetRequiredService<ITopicProducer<T>>();
        return producer.Produce(message, ct);
    }
}
