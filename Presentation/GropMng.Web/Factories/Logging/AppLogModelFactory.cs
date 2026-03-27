using AutoMapper;
using GropMng.Core.Interfaces.Services.Localization;
using GropMng.Core.Interfaces.Services.Logging;
using GropMng.Web.Areas.Admin.Models.Logging;
using GropMng.Web.Framework.Models.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Globalization;
using AppLogLevel = GropMng.Core.Domain.Logging.LogLevel;

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
                Selected = !searchModel.Level.HasValue
            }
        };

        foreach (var level in Enum.GetValues<AppLogLevel>())
        {
            var localizedLevel = await _localizationService.GetLocalizedEnumAsync(level, languageId);

            availableLogLevels.Add(new SelectListItem
            {
                Value = ((int)level).ToString(CultureInfo.InvariantCulture),
                Text = localizedLevel,
                Selected = searchModel.Level == level
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
            if (TryParseLogLevel(row.Level, out var parsedLevel))
            {
                row.Level = parsedLevel.ToString();
                row.LevelLocalized = await _localizationService.GetLocalizedEnumAsync(parsedLevel, languageId);
            }
            else
            {
                row.LevelLocalized = row.Level;
            }

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

    private static bool TryParseLogLevel(string? value, out AppLogLevel logLevel)
    {
        logLevel = default;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var normalized = value.Trim();
        if (Enum.TryParse<AppLogLevel>(normalized, true, out logLevel))
            return true;

        if (int.TryParse(normalized, NumberStyles.Integer, CultureInfo.InvariantCulture, out var numericLevel) &&
            Enum.IsDefined(typeof(AppLogLevel), numericLevel))
        {
            logLevel = (AppLogLevel)numericLevel;
            return true;
        }

        return false;
    }
}
