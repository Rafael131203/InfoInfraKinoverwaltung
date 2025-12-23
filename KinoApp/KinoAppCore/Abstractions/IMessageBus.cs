namespace KinoAppCore.Abstractions
{
    /// <summary>
    /// Publishes messages to the application's messaging infrastructure.
    /// </summary>
    public interface IMessageBus
    {
        /// <summary>
        /// Publishes a message to the bus.
        /// </summary>
        /// <typeparam name="T">The message type.</typeparam>
        /// <param name="message">The message instance to publish.</param>
        /// <param name="ct">A cancellation token used to cancel the publish operation.</param>
        /// <returns>A task that completes when the message has been accepted for publishing.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> is null.</exception>
        Task PublishAsync<T>(T message, CancellationToken ct = default) where T : class;
    }
}
