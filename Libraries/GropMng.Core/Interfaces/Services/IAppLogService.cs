using GropMng.Core.Domain.Logging;

namespace GropMng.Core.Interfaces.Services;

public interface IAppLogService
{
    Task InsertLogAsync(AppLog log);
    Task<IPagedList<AppLog>> GetAllLogsAsync(int pageIndex, int pageSize, string? level = null, DateTime? fromUtc = null, DateTime? toUtc = null);
    Task<int> GetLogsCountAsync(string? level = null, DateTime? fromUtc = null, DateTime? toUtc = null);
    Task<AppLog?> GetLogByIdAsync(int id);
    Task DeleteLogAsync(int id);
    Task ClearAllLogsAsync();
}
