namespace GropMng.Core.Events;

/// <summary>
/// Published after an entity has been inserted and persisted.
/// </summary>
public sealed class EntityInsertedEvent<TEntity> where TEntity : class
{
    public EntityInsertedEvent(TEntity entity)
    {
        Entity = entity;
    }

    public TEntity Entity { get; }
}