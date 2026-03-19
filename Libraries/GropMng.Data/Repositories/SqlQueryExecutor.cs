using GropMng.Core;
using GropMng.Core.Interfaces.Repositories;
using GropMng.Data.DbContext;
using Microsoft.EntityFrameworkCore;

namespace GropMng.Data.Repositories;

/// <summary>
/// Executes raw SQL queries and commands against the current <see cref="GropContext" />.
/// </summary>
public class SqlQueryExecutor : ISqlQueryExecutor
{
    private readonly GropContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlQueryExecutor" /> class.
    /// </summary>
    /// <param name="context">The EF Core database context used to execute SQL commands.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context" /> is <see langword="null" />.</exception>
    public SqlQueryExecutor(GropContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        _context = context;
    }

    /// <inheritdoc />
    public IQueryable<TEntity> EntityFromSqlRaw<TEntity>(string sql, params object[] parameters)
        where TEntity : BaseEntity
    {
        return _context.Set<TEntity>().FromSqlRaw(sql, parameters);
    }

    /// <inheritdoc />
    public IQueryable<TQuery> QueryFromSqlRaw<TQuery>(string sql, params object[] parameters)
        where TQuery : class
    {
        return _context.Database.SqlQueryRaw<TQuery>(sql, parameters);
    }

    /// <inheritdoc />
    public Task<int> ExecuteSqlRawAsync(string sql, CancellationToken cancellationToken = default)
    {
        return _context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> ExecuteSqlRawAsync(
        string sql,
        object[] parameters,
        bool ensureTransaction = true,
        int? timeout = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        var previousTimeout = _context.Database.GetCommandTimeout();
        if (timeout.HasValue)
            _context.Database.SetCommandTimeout(timeout.Value);

        try
        {
            if (!ensureTransaction)
                return await _context.Database.ExecuteSqlRawAsync(sql, parameters, cancellationToken);

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            var result = await _context.Database.ExecuteSqlRawAsync(sql, parameters, cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return result;
        }
        finally
        {
            _context.Database.SetCommandTimeout(previousTimeout);
        }
    }
}
