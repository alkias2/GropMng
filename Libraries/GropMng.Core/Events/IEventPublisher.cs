namespace GropMng.Core.Events;

/// <summary>
/// Publishes domain events to all registered consumers.
/// </summary>
public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent eventMessage, CancellationToken cancellationToken = default)
        where TEvent : class;
}