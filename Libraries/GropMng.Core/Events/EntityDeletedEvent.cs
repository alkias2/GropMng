namespace GropMng.Core.Events;

/// <summary>
/// Published after an entity has been deleted or soft-deleted and persisted.
/// </summary>
public sealed class EntityDeletedEvent<TEntity> where TEntity : class
{
    public EntityDeletedEvent(TEntity entity)
    {
        Entity = entity;
    }

    public TEntity Entity { get; }
}