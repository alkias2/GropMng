using AutoMapper;
using GropMng.Core.Interfaces.Services.Logging;
using GropMng.Web.Areas.Admin.Models.Logging;
using GropMng.Web.Framework.Models.Extensions;

namespace GropMng.Web.Factories.Logging;

/// <summary>
/// Default implementation of <see cref="IAppLogModelFactory"/>.
/// Delegates data access to <see cref="IAppLogService"/> and maps domain objects
/// to view models using <see cref="ModelExtensions.PrepareToGrid{TListModel,TRow,TObject}"/>.
/// </summary>
public class AppLogModelFactory : IAppLogModelFactory
{
    private readonly IAppLogService _appLogService;
    private readonly IMapper _mapper;

    public AppLogModelFactory(IAppLogService appLogService, IMapper mapper)
    {
        _appLogService = appLogService;
        _mapper = mapper;
    }

    /// <inheritdoc />
    public AppLogSearchModel PrepareSearchModel(AppLogSearchModel? searchModel = null)
    {
        searchModel ??= new AppLogSearchModel();
        searchModel.SetGridPageSize();
        return searchModel;
    }

    /// <inheritdoc />
    public async Task<AppLogListModel> PrepareListModelAsync(AppLogSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        // Repository expects 0-based page index
        var pageIndex = searchModel.Page - 1;

        var logs = await _appLogService.GetAllLogsAsync(
            pageIndex,
            searchModel.PageSize,
            searchModel.Level,
            searchModel.FromDate,
            searchModel.ToDate);

        var listModel = new AppLogListModel();
        return listModel.PrepareToGrid(searchModel, logs, () =>
            logs.Select(x => _mapper.Map<AppLogRowModel>(x)));
    }
}
