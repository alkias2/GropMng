namespace GropMng.Core.Events;

/// <summary>
/// Handles a published domain event.
/// </summary>
/// <typeparam name="TEvent">The event type.</typeparam>
public interface IConsumer<in TEvent> where TEvent : class
{
    Task HandleEventAsync(TEvent eventMessage, CancellationToken cancellationToken = default);
}