using GropMng.Core.Events;
using Microsoft.Extensions.DependencyInjection;

namespace GropMng.Services.Events;

/// <summary>
/// Default in-process event publisher that dispatches events to all registered consumers.
/// </summary>
public sealed class DefaultEventPublisher : IEventPublisher
{
    private readonly IServiceProvider _serviceProvider;

    public DefaultEventPublisher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task PublishAsync<TEvent>(TEvent eventMessage, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        ArgumentNullException.ThrowIfNull(eventMessage);

        var consumers = _serviceProvider.GetServices<IConsumer<TEvent>>();
        foreach (var consumer in consumers)
            await consumer.HandleEventAsync(eventMessage, cancellationToken);
    }
}