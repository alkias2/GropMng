using GropMng.Core.Caching;
using GropMng.Core.Events;

namespace GropMng.Services.Caching;

/// <summary>
/// Base class for entity-driven cache invalidation consumers.
/// </summary>
public abstract class BaseCacheEventConsumer<TEntity> :
    IConsumer<EntityInsertedEvent<TEntity>>,
    IConsumer<EntityUpdatedEvent<TEntity>>,
    IConsumer<EntityDeletedEvent<TEntity>>
    where TEntity : class
{
    protected BaseCacheEventConsumer(IGropStaticCacheManager cacheManager)
    {
        CacheManager = cacheManager;
    }

    protected IGropStaticCacheManager CacheManager { get; }

    public Task HandleEventAsync(EntityInsertedEvent<TEntity> eventMessage, CancellationToken cancellationToken = default)
        => ClearCacheAsync(eventMessage.Entity, EntityEventType.Insert, cancellationToken);

    public Task HandleEventAsync(EntityUpdatedEvent<TEntity> eventMessage, CancellationToken cancellationToken = default)
        => ClearCacheAsync(eventMessage.Entity, EntityEventType.Update, cancellationToken);

    public Task HandleEventAsync(EntityDeletedEvent<TEntity> eventMessage, CancellationToken cancellationToken = default)
        => ClearCacheAsync(eventMessage.Entity, EntityEventType.Delete, cancellationToken);

    protected abstract Task ClearCacheAsync(TEntity entity, EntityEventType eventType, CancellationToken cancellationToken);

    protected enum EntityEventType
    {
        Insert,
        Update,
        Delete
    }
}