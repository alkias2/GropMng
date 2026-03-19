namespace GropMng.Core.Interfaces.Repositories;

/// <summary>
/// Defines an abstraction for executing raw SQL queries and commands through the data access layer.
/// </summary>
public interface ISqlQueryExecutor
{
    /// <summary>
    /// Creates a raw SQL query for an entity type mapped by the current DbContext.
    /// </summary>
    /// <typeparam name="TEntity">The mapped entity type returned by the query.</typeparam>
    /// <param name="sql">The SQL statement to execute.</param>
    /// <param name="parameters">The SQL parameters to apply to the command.</param>
    /// <returns>An <see cref="IQueryable{T}" /> that can be further composed before execution.</returns>
    IQueryable<TEntity> EntityFromSqlRaw<TEntity>(string sql, params object[] parameters)
        where TEntity : BaseEntity;

    /// <summary>
    /// Creates a raw SQL query for an unmapped or projection type supported by EF Core SQL queries.
    /// </summary>
    /// <typeparam name="TQuery">The result type returned by the query.</typeparam>
    /// <param name="sql">The SQL statement to execute.</param>
    /// <param name="parameters">The SQL parameters to apply to the command.</param>
    /// <returns>An <see cref="IQueryable{T}" /> that can be further composed before execution.</returns>
    IQueryable<TQuery> QueryFromSqlRaw<TQuery>(string sql, params object[] parameters)
        where TQuery : class;

    /// <summary>
    /// Executes a raw SQL command without additional parameters.
    /// </summary>
    /// <param name="sql">The SQL command to execute.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The number of rows affected by the command.</returns>
    Task<int> ExecuteSqlRawAsync(string sql, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a raw SQL command with parameters and optional transaction handling.
    /// </summary>
    /// <param name="sql">The SQL command to execute.</param>
    /// <param name="parameters">The SQL parameters to apply to the command.</param>
    /// <param name="ensureTransaction">A value indicating whether the command should execute inside a new transaction.</param>
    /// <param name="timeout">An optional command timeout in seconds.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The number of rows affected by the command.</returns>
    Task<int> ExecuteSqlRawAsync(
        string sql,
        object[] parameters,
        bool ensureTransaction = true,
        int? timeout = null,
        CancellationToken cancellationToken = default);
}
