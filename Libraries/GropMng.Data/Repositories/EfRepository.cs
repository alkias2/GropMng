using System.Linq.Expressions;
using System.Reflection;
using GropMng.Core;
using GropMng.Core.Domain.Garden;
using GropMng.Core.Events;
using GropMng.Core.Interfaces.Repositories;
using GropMng.Data.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace GropMng.Data.Repositories;

/// <summary>
/// Provides an Entity Framework Core implementation of <see cref="IRepository{TEntity}" />.
/// </summary>
/// <typeparam name="TEntity">The entity type managed by the repository.</typeparam>
public class EfRepository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity
{
    private static readonly MethodInfo PublishAsyncMethod = typeof(IEventPublisher)
        .GetMethod(nameof(IEventPublisher.PublishAsync))
        ?? throw new InvalidOperationException($"Unable to resolve {nameof(IEventPublisher)}.{nameof(IEventPublisher.PublishAsync)}.");

    private readonly GropContext _context;
    private readonly DbSet<TEntity> _dbSet;
    private readonly IEventPublisher _eventPublisher;

    /// <summary>
    /// Initializes a new instance of the <see cref="EfRepository{TEntity}" /> class.
    /// </summary>
    /// <param name="context">The EF Core database context used by the repository.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context" /> is <see langword="null" />.</exception>
    public EfRepository(GropContext context, IEventPublisher eventPublisher)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(eventPublisher);

        _context = context;
        _dbSet = _context.Set<TEntity>();
        _eventPublisher = eventPublisher;
    }

    /// <inheritdoc />
    public IQueryable<TEntity> Table => ApplySoftDeleteFilter(_dbSet.AsQueryable(), includeDeleted: false);

    /// <inheritdoc />
    public IQueryable<TEntity> TableNoTracking => ApplySoftDeleteFilter(_dbSet.AsNoTracking(), includeDeleted: false);

    /// <inheritdoc />
    public Task<TEntity?> GetByIdAsync(int id, bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        return BuildQuery(includeDeleted, asNoTracking: false)
            .FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TEntity>> GetByIdsAsync(IReadOnlyCollection<int> ids, bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
            return Array.Empty<TEntity>();

        var items = await BuildQuery(includeDeleted, asNoTracking: true)
            .Where(entity => ids.Contains(entity.Id))
            .ToListAsync(cancellationToken);

        return items;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TEntity>> GetAllAsync(
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? queryShaper = null,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(includeDeleted, asNoTracking: true);

        if (queryShaper is not null)
            query = queryShaper(query);

        return await query.ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IPagedList<TEntity>> GetPagedAsync(
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? queryShaper = null,
        int pageIndex = 0,
        int pageSize = int.MaxValue,
        bool getOnlyTotalCount = false,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default)
    {
        if (pageIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(pageIndex));

        if (pageSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(pageSize));

        var query = BuildQuery(includeDeleted, asNoTracking: true);

        if (queryShaper is not null)
            query = queryShaper(query);

        var totalCount = await query.CountAsync(cancellationToken);

        if (getOnlyTotalCount)
            return new PagedList<TEntity>(Array.Empty<TEntity>(), pageIndex, pageSize, totalCount);

        var items = await query
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedList<TEntity>(items, pageIndex, pageSize, totalCount);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool includeDeleted = false,
        bool asNoTracking = true,
        CancellationToken cancellationToken = default)
    {
        return await BuildQuery(includeDeleted, asNoTracking)
            .Where(predicate)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool includeDeleted = false,
        bool asNoTracking = true,
        CancellationToken cancellationToken = default)
    {
        return BuildQuery(includeDeleted, asNoTracking)
            .FirstOrDefaultAsync(predicate, cancellationToken);
    }

    /// <inheritdoc />
    public Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default)
    {
        return BuildQuery(includeDeleted, asNoTracking: true)
            .AnyAsync(predicate, cancellationToken);
    }

    /// <inheritdoc />
    public Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(includeDeleted, asNoTracking: true);

        if (predicate is not null)
            query = query.Where(predicate);

        return query.CountAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TEntity> CreateAsync(TEntity entity, bool saveNow = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        await _dbSet.AddAsync(entity, cancellationToken);

        if (saveNow)
            await SaveChangesAsync(cancellationToken);

        return entity;
    }

    /// <inheritdoc />
    public async Task CreateAsync(IReadOnlyCollection<TEntity> entities, bool saveNow = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);

        if (entities.Count == 0)
            return;

        await _dbSet.AddRangeAsync(entities, cancellationToken);

        if (saveNow)
            await SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TEntity> UpdateAsync(TEntity entity, bool saveNow = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        _dbSet.Update(entity);

        if (saveNow)
            await SaveChangesAsync(cancellationToken);

        return entity;
    }

    /// <inheritdoc />
    public async Task UpdateAsync(IReadOnlyCollection<TEntity> entities, bool saveNow = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);

        if (entities.Count == 0)
            return;

        _dbSet.UpdateRange(entities);

        if (saveNow)
            await SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(TEntity entity, bool softDelete = true, bool saveNow = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        if (softDelete && entity is AuditableEntity auditableEntity)
        {
            MarkSoftDeleted(auditableEntity);
            _dbSet.Update(entity);
        }
        else
        {
            _dbSet.Remove(entity);
        }

        if (saveNow)
            await SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(IReadOnlyCollection<TEntity> entities, bool softDelete = true, bool saveNow = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);

        if (entities.Count == 0)
            return;

        if (softDelete && typeof(AuditableEntity).IsAssignableFrom(typeof(TEntity)))
        {
            foreach (var entity in entities)
            {
                if (entity is AuditableEntity auditableEntity)
                    MarkSoftDeleted(auditableEntity);
            }

            _dbSet.UpdateRange(entities);
        }
        else
        {
            _dbSet.RemoveRange(entities);
        }

        if (saveNow)
            await SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> DeleteAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool softDelete = true,
        bool saveNow = true,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        var entities = await BuildQuery(includeDeleted: true, asNoTracking: false)
            .Where(predicate)
            .ToListAsync(cancellationToken);

        if (entities.Count == 0)
            return 0;

        await DeleteAsync(entities, softDelete, saveNow, cancellationToken);

        return entities.Count;
    }

    /// <inheritdoc />
    public Task<int> SoftDeleteAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool saveNow = true,
        CancellationToken cancellationToken = default)
    {
        if (!typeof(AuditableEntity).IsAssignableFrom(typeof(TEntity)))
            throw new InvalidOperationException($"Soft delete is available only for {nameof(AuditableEntity)} entities.");

        return DeleteAsync(predicate, softDelete: true, saveNow: saveNow, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var pendingEvents = GetPendingEntityEvents();

        await _context.SaveChangesAsync(cancellationToken);

        foreach (var pendingEvent in pendingEvents)
            await pendingEvent(cancellationToken);
    }

    private List<Func<CancellationToken, Task>> GetPendingEntityEvents()
    {
        var pendingEvents = new List<Func<CancellationToken, Task>>();

        foreach (var entry in _context.ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                pendingEvents.Add(CreatePublishCallback(typeof(EntityInsertedEvent<>), entry.Entity));
                continue;
            }

            if (entry.State == EntityState.Deleted)
            {
                pendingEvents.Add(CreatePublishCallback(typeof(EntityDeletedEvent<>), entry.Entity));
                continue;
            }

            if (entry.State != EntityState.Modified)
                continue;

            if (entry.Entity is AuditableEntity && IsSoftDeleted(entry))
            {
                pendingEvents.Add(CreatePublishCallback(typeof(EntityDeletedEvent<>), entry.Entity));
                continue;
            }

            pendingEvents.Add(CreatePublishCallback(typeof(EntityUpdatedEvent<>), entry.Entity));
        }

        return pendingEvents;
    }

    private Func<CancellationToken, Task> CreatePublishCallback(Type eventTypeDefinition, BaseEntity entity)
    {
        var entityType = entity.GetType();
        var eventType = eventTypeDefinition.MakeGenericType(entityType);
        var eventMessage = Activator.CreateInstance(eventType, entity)
            ?? throw new InvalidOperationException($"Unable to create event instance for {eventType.Name}.");

        return cancellationToken => PublishEventAsync(eventType, eventMessage, cancellationToken);
    }

    private Task PublishEventAsync(Type eventType, object eventMessage, CancellationToken cancellationToken)
    {
        var publishMethod = PublishAsyncMethod.MakeGenericMethod(eventType);

        return (Task)(publishMethod.Invoke(_eventPublisher, new object?[] { eventMessage, cancellationToken })
            ?? throw new InvalidOperationException($"Event publication returned null for {eventType.Name}."));
    }

    private static bool IsSoftDeleted(EntityEntry<BaseEntity> entry)
    {
        if (entry.Entity is not AuditableEntity)
            return false;

        var originalValue = entry.OriginalValues.GetValue<bool>(nameof(AuditableEntity.IsDeleted));
        var currentValue = entry.CurrentValues.GetValue<bool>(nameof(AuditableEntity.IsDeleted));

        return !originalValue && currentValue;
    }

    /// <summary>
    /// Builds the base query for the current entity type and applies change-tracking and soft-delete behavior.
    /// </summary>
    /// <param name="includeDeleted">A value indicating whether soft-deleted entities should be included when supported by the entity type.</param>
    /// <param name="asNoTracking">A value indicating whether the returned query should disable EF Core tracking.</param>
    /// <returns>An <see cref="IQueryable{T}" /> configured for the requested behavior.</returns>
    private IQueryable<TEntity> BuildQuery(bool includeDeleted, bool asNoTracking)
    {
        var query = asNoTracking ? _dbSet.AsNoTracking() : _dbSet.AsQueryable();
        return ApplySoftDeleteFilter(query, includeDeleted);
    }

    /// <summary>
    /// Applies a soft-delete filter to the supplied query when the entity type derives from <see cref="AuditableEntity" />.
    /// </summary>
    /// <param name="query">The source query to filter.</param>
    /// <param name="includeDeleted">A value indicating whether soft-deleted entities should remain in the result set.</param>
    /// <returns>The original query or a filtered query excluding soft-deleted records.</returns>
    private static IQueryable<TEntity> ApplySoftDeleteFilter(IQueryable<TEntity> query, bool includeDeleted)
    {
        if (includeDeleted || !typeof(AuditableEntity).IsAssignableFrom(typeof(TEntity)))
            return query;

        var parameter = Expression.Parameter(typeof(TEntity), "entity");
        var isDeletedProperty = Expression.Call(
            typeof(EF),
            nameof(EF.Property),
            new[] { typeof(bool) },
            parameter,
            Expression.Constant(nameof(AuditableEntity.IsDeleted)));

        var notDeleted = Expression.Equal(isDeletedProperty, Expression.Constant(false));
        var filter = Expression.Lambda<Func<TEntity, bool>>(notDeleted, parameter);

        return query.Where(filter);
    }

    /// <summary>
    /// Marks an auditable entity as soft-deleted and updates its audit timestamps.
    /// </summary>
    /// <param name="entity">The entity to mark as soft-deleted.</param>
    private static void MarkSoftDeleted(AuditableEntity entity)
    {
        entity.IsDeleted = true;
        entity.DeletedAtUtc = DateTime.UtcNow;
        entity.UpdatedAtUtc = DateTime.UtcNow;
    }
}
