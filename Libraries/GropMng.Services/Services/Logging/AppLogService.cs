using GropMng.Core;
using GropMng.Core.Domain.Logging;
using GropMng.Core.Interfaces.Services.Logging;
using GropMng.Data.DbContext;
using Microsoft.EntityFrameworkCore;

namespace GropMng.Services.Services.Logging;

/// <summary>
/// Represents the AppLogService component.
/// Defines responsibilities and data relevant to its role in the GropMng solution.
/// </summary>
public class AppLogService : IAppLogService
{
    private readonly GropContext _sqlServerContext;

    public AppLogService(GropContext sqlServerContext)
    {
        _sqlServerContext = sqlServerContext;
    }

    public async Task InsertLogAsync(AppLog log)
    {
        await _sqlServerContext.AppLogs.AddAsync(log);
        await _sqlServerContext.SaveChangesAsync();
    }

    public async Task<IPagedList<AppLog>> GetAllLogsAsync(
        int pageIndex,
        int pageSize,
        string? level = null,
        DateTime? fromUtc = null,
        DateTime? toUtc = null)
    {
        var query = BuildFilteredQuery(level, fromUtc, toUtc)
            .OrderByDescending(x => x.Timestamp)
            .ThenByDescending(x => x.Id);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedList<AppLog>(items, pageIndex, pageSize, totalCount);
    }

    public Task<int> GetLogsCountAsync(string? level = null, DateTime? fromUtc = null, DateTime? toUtc = null)
    {
        return BuildFilteredQuery(level, fromUtc, toUtc).CountAsync();
    }

    public Task<AppLog?> GetLogByIdAsync(int id)
    {
        return _sqlServerContext.AppLogs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task DeleteLogAsync(int id)
    {
        var entity = await _sqlServerContext.AppLogs.FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null)
            return;

        _sqlServerContext.AppLogs.Remove(entity);
        await _sqlServerContext.SaveChangesAsync();
    }

    public async Task DeleteLogsAsync(IEnumerable<int> ids)
    {
        ArgumentNullException.ThrowIfNull(ids);

        var idList = ids.Distinct().ToList();
        if (!idList.Any())
            return;

        var entities = await _sqlServerContext.AppLogs
            .Where(x => idList.Contains(x.Id))
            .ToListAsync();

        if (!entities.Any())
            return;

        _sqlServerContext.AppLogs.RemoveRange(entities);
        await _sqlServerContext.SaveChangesAsync();
    }

    public async Task ClearAllLogsAsync()
    {
        await _sqlServerContext.Database.ExecuteSqlRawAsync("DELETE FROM [AppLog]");
    }

    private IQueryable<AppLog> BuildFilteredQuery(string? level, DateTime? fromUtc, DateTime? toUtc)
    {
        var query = _sqlServerContext.AppLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(level))
            query = query.Where(x => x.Level == level);

        if (fromUtc.HasValue)
            query = query.Where(x => x.Timestamp >= fromUtc.Value);

        if (toUtc.HasValue)
            query = query.Where(x => x.Timestamp <= toUtc.Value);

        return query;
    }
}
