using System.Linq.Expressions;

namespace GropMng.Core.Interfaces.Repositories;

/// <summary>
/// Defines a generic repository contract for querying and persisting entities.
/// </summary>
/// <typeparam name="TEntity">The entity type managed by the repository.</typeparam>
public interface IRepository<TEntity> where TEntity : BaseEntity
{
    /// <summary>
    /// Gets a queryable source for tracked entities excluding soft-deleted records when supported by the entity type.
    /// </summary>
    IQueryable<TEntity> Table { get; }

    /// <summary>
    /// Gets a queryable source for non-tracked entities excluding soft-deleted records when supported by the entity type.
    /// </summary>
    IQueryable<TEntity> TableNoTracking { get; }

    /// <summary>
    /// Retrieves a single entity by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the entity to retrieve.</param>
    /// <param name="includeDeleted">A value indicating whether soft-deleted records should be included when the entity type supports soft deletion.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The matching entity when found; otherwise, <see langword="null" />.</returns>
    Task<TEntity?> GetByIdAsync(int id, bool includeDeleted = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves entities matching the supplied identifiers.
    /// </summary>
    /// <param name="ids">The identifiers of the entities to retrieve.</param>
    /// <param name="includeDeleted">A value indicating whether soft-deleted records should be included when the entity type supports soft deletion.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A read-only list containing the entities that match the supplied identifiers.</returns>
    Task<IReadOnlyList<TEntity>> GetByIdsAsync(IReadOnlyCollection<int> ids, bool includeDeleted = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all entities after optionally applying additional query shaping.
    /// </summary>
    /// <param name="queryShaper">An optional delegate used to apply filtering, ordering, projection-related includes, or other query composition before execution.</param>
    /// <param name="includeDeleted">A value indicating whether soft-deleted records should be included when the entity type supports soft deletion.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A read-only list containing the resulting entities.</returns>
    Task<IReadOnlyList<TEntity>> GetAllAsync(
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? queryShaper = null,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paged result set after optionally applying additional query shaping.
    /// </summary>
    /// <param name="queryShaper">An optional delegate used to apply filtering, ordering, projection-related includes, or other query composition before paging is applied.</param>
    /// <param name="pageIndex">The zero-based page index.</param>
    /// <param name="pageSize">The number of items to include in the page.</param>
    /// <param name="getOnlyTotalCount">A value indicating whether only the total record count should be calculated without loading page items.</param>
    /// <param name="includeDeleted">A value indicating whether soft-deleted records should be included when the entity type supports soft deletion.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A paged list containing the requested page metadata and items.</returns>
    Task<IPagedList<TEntity>> GetPagedAsync(
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? queryShaper = null,
        int pageIndex = 0,
        int pageSize = int.MaxValue,
        bool getOnlyTotalCount = false,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves entities matching the supplied predicate.
    /// </summary>
    /// <param name="predicate">The filter predicate that entities must satisfy.</param>
    /// <param name="includeDeleted">A value indicating whether soft-deleted records should be included when the entity type supports soft deletion.</param>
    /// <param name="asNoTracking">A value indicating whether the query should be executed without EF Core change tracking.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A read-only list containing the matching entities.</returns>
    Task<IReadOnlyList<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool includeDeleted = false,
        bool asNoTracking = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the first entity that matches the supplied predicate.
    /// </summary>
    /// <param name="predicate">The filter predicate that entities must satisfy.</param>
    /// <param name="includeDeleted">A value indicating whether soft-deleted records should be included when the entity type supports soft deletion.</param>
    /// <param name="asNoTracking">A value indicating whether the query should be executed without EF Core change tracking.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The first matching entity when found; otherwise, <see langword="null" />.</returns>
    Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool includeDeleted = false,
        bool asNoTracking = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether any entity matches the supplied predicate.
    /// </summary>
    /// <param name="predicate">The filter predicate that entities must satisfy.</param>
    /// <param name="includeDeleted">A value indicating whether soft-deleted records should be included when the entity type supports soft deletion.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns><see langword="true" /> when at least one entity matches the predicate; otherwise, <see langword="false" />.</returns>
    Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts entities, optionally restricted by a predicate.
    /// </summary>
    /// <param name="predicate">An optional filter predicate that entities must satisfy in order to be counted.</param>
    /// <param name="includeDeleted">A value indicating whether soft-deleted records should be included when the entity type supports soft deletion.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The total number of matching entities.</returns>
    Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new entity in the underlying data store.
    /// </summary>
    /// <param name="entity">The entity to create.</param>
    /// <param name="saveNow">A value indicating whether changes should be committed immediately.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The created entity instance.</returns>
    Task<TEntity> CreateAsync(TEntity entity, bool saveNow = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates multiple entities in the underlying data store.
    /// </summary>
    /// <param name="entities">The entities to create.</param>
    /// <param name="saveNow">A value indicating whether changes should be committed immediately.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    Task CreateAsync(IReadOnlyCollection<TEntity> entities, bool saveNow = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity in the underlying data store.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <param name="saveNow">A value indicating whether changes should be committed immediately.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The updated entity instance.</returns>
    Task<TEntity> UpdateAsync(TEntity entity, bool saveNow = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates multiple entities in the underlying data store.
    /// </summary>
    /// <param name="entities">The entities to update.</param>
    /// <param name="saveNow">A value indicating whether changes should be committed immediately.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    Task UpdateAsync(IReadOnlyCollection<TEntity> entities, bool saveNow = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a single entity from the underlying data store.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    /// <param name="softDelete">A value indicating whether a supported entity should be soft-deleted instead of physically removed.</param>
    /// <param name="saveNow">A value indicating whether changes should be committed immediately.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    Task DeleteAsync(TEntity entity, bool softDelete = true, bool saveNow = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes multiple entities from the underlying data store.
    /// </summary>
    /// <param name="entities">The entities to delete.</param>
    /// <param name="softDelete">A value indicating whether supported entities should be soft-deleted instead of physically removed.</param>
    /// <param name="saveNow">A value indicating whether changes should be committed immediately.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    Task DeleteAsync(IReadOnlyCollection<TEntity> entities, bool softDelete = true, bool saveNow = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes entities that match the supplied predicate.
    /// </summary>
    /// <param name="predicate">The filter predicate used to select entities for deletion.</param>
    /// <param name="softDelete">A value indicating whether supported entities should be soft-deleted instead of physically removed.</param>
    /// <param name="saveNow">A value indicating whether changes should be committed immediately.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The number of deleted entities.</returns>
    Task<int> DeleteAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool softDelete = true,
        bool saveNow = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft-deletes entities that match the supplied predicate.
    /// </summary>
    /// <param name="predicate">The filter predicate used to select entities for soft deletion.</param>
    /// <param name="saveNow">A value indicating whether changes should be committed immediately.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The number of soft-deleted entities.</returns>
    Task<int> SoftDeleteAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool saveNow = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists all pending changes tracked by the repository context.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
