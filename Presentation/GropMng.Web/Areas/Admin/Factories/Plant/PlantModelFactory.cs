using AutoMapper;
using GropMng.Core.Domain.Garden.Enums;
using GropMng.Core.Domain.Garden.Plants;
using GropMng.Core.Interfaces.Services.Garden.Plants;
using GropMng.Core.Interfaces.Services.Localization;
using GropMng.Web.Areas.Admin.Models.Plant;
using GropMng.Web.Extensions;
using GropMng.Web.Framework.Models.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using DomainPlant = GropMng.Core.Domain.Garden.Plants.Plant;

namespace GropMng.Web.Areas.Admin.Factories.Plant;

/// <summary>
/// Default implementation for Plant admin model preparation and persistence orchestration.
/// </summary>
public class PlantModelFactory : IPlantModelFactory
{
    #region Fields

    private readonly IPlantService _plantService;
    private readonly IMapper _mapper;
    private readonly ILocalizationService _localizationService;

    #endregion

    #region Ctor

    public PlantModelFactory(IPlantService plantService, IMapper mapper, ILocalizationService localizationService)
    {
        _plantService = plantService;
        _mapper = mapper;
        _localizationService = localizationService;
    }

    #endregion

    #region Public

    /// <inheritdoc />
    public async Task<PlantSearchModel> PrepareSearchModelAsync(PlantSearchModel? searchModel = null, CancellationToken cancellationToken = default)
    {
        searchModel ??= new PlantSearchModel();
        searchModel.SetGridPageSize();

        searchModel.AvailableCategories = await _localizationService.GetLocalizedEnumSelectListAsync<PlantCategory>(
            "admin.plant.category",
            "admin.plant.category.all");

        return searchModel;
    }

    /// <inheritdoc />
    public async Task<PlantListModel> PrepareListModelAsync(PlantSearchModel searchModel, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        var pageIndex = searchModel.Page - 1;
        var plants = await _plantService.GetPlantsAsync(
            searchModel.SearchTerm,
            searchModel.Category,
            pageIndex,
            searchModel.PageSize,
            cancellationToken);

        var edibleText = await _localizationService.GetResourceAsync("admin.plant.flags.edible");
        var medicinalText = await _localizationService.GetResourceAsync("admin.plant.flags.medicinal");
        var toxicText = await _localizationService.GetResourceAsync("admin.plant.flags.toxic");
        var noneText = await _localizationService.GetResourceAsync("common.none");

        var mappedRows = plants.Select(entity => _mapper.Map<PlantRowModel>(entity)).ToList();

        foreach (var row in mappedRows)
        {
            if (Enum.TryParse<PlantCategory>(row.Category, true, out var parsedCategory))
                row.CategoryLocalized = await _localizationService.GetLocalizedEnumAsync(parsedCategory);
            else
                row.CategoryLocalized = row.Category;

            var flags = new List<string>();

            if (row.IsEdible)
                flags.Add(edibleText);

            if (row.IsMedicinal)
                flags.Add(medicinalText);

            if (row.IsToxic)
                flags.Add(toxicText);

            row.FlagsSummary = flags.Count > 0
                ? string.Join(", ", flags)
                : noneText;
        }

        var listModel = new PlantListModel();
        return listModel.PrepareToGrid(searchModel, plants, () =>
            mappedRows);
    }

    /// <inheritdoc />
    public Task<PlantModel> PrepareCreateModelAsync(CancellationToken cancellationToken = default)
    {
        var model = PrepareLookups(new PlantModel());
        return Task.FromResult(model);
    }

    /// <inheritdoc />
    public async Task<PlantModel?> PrepareEditModelAsync(int plantId, CancellationToken cancellationToken = default)
    {
        var plant = await _plantService.GetPlantByIdAsync(plantId, cancellationToken: cancellationToken);
        if (plant == null)
            return null;

        var model = _mapper.Map<PlantModel>(plant);
        return PrepareLookups(model);
    }

    /// <inheritdoc />
    public async Task SaveCreateAsync(PlantModel model, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);
        var entity = _mapper.Map<DomainPlant>(model);
        await _plantService.CreatePlantAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> SaveEditAsync(PlantModel model, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);

        var existing = await _plantService.GetPlantByIdAsync(model.Id, cancellationToken: cancellationToken);
        if (existing == null)
            return false;

        _mapper.Map(model, existing);
        await _plantService.UpdatePlantAsync(existing, cancellationToken);
        return true;
    }

    #endregion

    #region Privates

    private static PlantModel PrepareLookups(PlantModel model)
    {
        model.AvailableCategories = BuildEnumList<PlantCategory>();
        model.AvailableGrowthTypes = BuildNullableEnumList<PlantGrowthType>();
        model.AvailableSunRequirements = BuildNullableEnumList<PlantSunRequirement>();
        model.AvailableWaterRequirements = BuildNullableEnumList<PlantWaterRequirement>();
        return model;
    }

    private static IList<SelectListItem> BuildEnumList<TEnum>() where TEnum : struct, Enum
    {
        return Enum.GetValues<TEnum>()
            .Select(value => new SelectListItem
            {
                Value = value.ToString(),
                Text = value.ToString()
            })
            .ToList();
    }

    private static IList<SelectListItem> BuildNullableEnumList<TEnum>() where TEnum : struct, Enum
    {
        var items = new List<SelectListItem>
        {
            new() { Value = string.Empty, Text = "-- None --" }
        };

        items.AddRange(BuildEnumList<TEnum>());
        return items;
    }

    #endregion
}
