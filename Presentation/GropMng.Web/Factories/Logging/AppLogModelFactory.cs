using AutoMapper;
using GropMng.Core.Interfaces.Services.Localization;
using GropMng.Core.Interfaces.Services.Logging;
using GropMng.Web.Areas.Admin.Models.Logging;
using GropMng.Web.Framework.Models.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;

namespace GropMng.Web.Factories.Logging;

/// <summary>
/// Default implementation of <see cref="IAppLogModelFactory"/>.
/// Delegates data access to <see cref="IAppLogService"/> and maps domain objects
/// to view models using <see cref="ModelExtensions.PrepareToGrid{TListModel,TRow,TObject}"/>.
/// </summary>
public class AppLogModelFactory : IAppLogModelFactory
{
    private static readonly string[] SupportedLogLevels =
    [
        "Trace",
        "Debug",
        "Information",
        "Warning",
        "Error",
        "Critical"
    ];

    private readonly IAppLogService _appLogService;
    private readonly IMapper _mapper;
    private readonly ILocalizationService _localizationService;
    private readonly ILanguageService _languageService;

    public AppLogModelFactory(
        IAppLogService appLogService,
        IMapper mapper,
        ILocalizationService localizationService,
        ILanguageService languageService)
    {
        _appLogService = appLogService;
        _mapper = mapper;
        _localizationService = localizationService;
        _languageService = languageService;
    }

    /// <inheritdoc />
    public async Task<AppLogSearchModel> PrepareSearchModelAsync(AppLogSearchModel? searchModel = null, CancellationToken cancellationToken = default)
    {
        searchModel ??= new AppLogSearchModel();
        searchModel.SetGridPageSize();

        var languageId = await ResolveCurrentLanguageIdAsync(cancellationToken);
        var allLevelsText = await _localizationService.GetResourceAsync(
            "admin.applog.filters.alllevels",
            languageId,
            logIfNotFound: false,
            defaultValue: "All levels");

        var availableLogLevels = new List<SelectListItem>
        {
            new()
            {
                Value = string.Empty,
                Text = allLevelsText,
                Selected = string.IsNullOrWhiteSpace(searchModel.Level)
            }
        };

        foreach (var level in SupportedLogLevels)
        {
            var levelKey = $"admin.applog.level.{level.ToLowerInvariant()}";
            var localizedLevel = await _localizationService.GetResourceAsync(
                levelKey,
                languageId,
                logIfNotFound: false,
                defaultValue: level);

            availableLogLevels.Add(new SelectListItem
            {
                Value = level,
                Text = localizedLevel,
                Selected = string.Equals(searchModel.Level, level, StringComparison.OrdinalIgnoreCase)
            });
        }

        searchModel.AvailableLogLevels = availableLogLevels;
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
            searchModel.Message,
            searchModel.FromDate,
            searchModel.ToDate);

        var languageId = await ResolveCurrentLanguageIdAsync();
        var mappedRows = logs.Select(x => _mapper.Map<AppLogRowModel>(x)).ToList();

        foreach (var row in mappedRows)
        {
            var levelKey = $"admin.applog.level.{row.Level.ToLowerInvariant()}";
            row.LevelLocalized = await _localizationService.GetResourceAsync(
                levelKey,
                languageId,
                logIfNotFound: false,
                defaultValue: row.Level);
            row.TimestampLocalized = row.Timestamp.ToLocalTime().ToString("g", CultureInfo.CurrentUICulture);
        }

        var listModel = new AppLogListModel();
        return listModel.PrepareToGrid(searchModel, logs, () =>
            mappedRows);
    }

    private async Task<int> ResolveCurrentLanguageIdAsync(CancellationToken cancellationToken = default)
    {
        var currentCulture = CultureInfo.CurrentUICulture;
        var languages = await _languageService.GetAllLanguagesAsync(showHidden: false, cancellationToken: cancellationToken);

        var exactCultureMatch = languages.FirstOrDefault(x =>
            string.Equals(x.LanguageCulture, currentCulture.Name, StringComparison.OrdinalIgnoreCase));
        if (exactCultureMatch is not null)
            return exactCultureMatch.Id;

        var twoLetterCode = currentCulture.TwoLetterISOLanguageName;
        var seoCodeMatch = languages.FirstOrDefault(x =>
            string.Equals(x.UniqueSeoCode, twoLetterCode, StringComparison.OrdinalIgnoreCase));
        if (seoCodeMatch is not null)
            return seoCodeMatch.Id;

        var defaultLanguage = await _languageService.GetDefaultLanguageAsync(cancellationToken);
        return defaultLanguage.Id;
    }
}
