namespace GropMng.Core.Events;

/// <summary>
/// Published after an entity has been updated and persisted.
/// </summary>
public sealed class EntityUpdatedEvent<TEntity> where TEntity : class
{
    public EntityUpdatedEvent(TEntity entity)
    {
        Entity = entity;
    }

    public TEntity Entity { get; }
}