using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using KinoAppCore.Abstractions;

namespace KinoAppService.Messaging
{
    /// <summary>
    /// Message bus adapter that publishes messages via MassTransit Kafka topic producers.
    /// </summary>
    /// <remarks>
    /// This adapter keeps Core decoupled from MassTransit by exposing a small publish-only abstraction
    /// (<see cref="IMessageBus"/>).
    /// </remarks>
    public sealed class MassTransitKafkaMessageBus : IMessageBus
    {
        private readonly IServiceProvider _services;

        /// <summary>
        /// Creates a new <see cref="MassTransitKafkaMessageBus"/>.
        /// </summary>
        /// <param name="services">Service provider used to resolve topic producers.</param>
        public MassTransitKafkaMessageBus(IServiceProvider services)
        {
            _services = services;
        }

        /// <inheritdoc />
        public Task PublishAsync<T>(T message, CancellationToken ct = default) where T : class
        {
            var producer = _services.GetRequiredService<ITopicProducer<T>>();
            return producer.Produce(message, ct);
        }
    }
}
