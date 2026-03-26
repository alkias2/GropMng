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

namespace GropMng.Web.Factories.Plant;

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

        var titles = new PlantGridColumnTitles
        {
            Id             = await _localizationService.GetResourceAsync("admin.plant.grid.id"),
            CommonName     = await _localizationService.GetResourceAsync("admin.plant.fields.commonname"),
            ScientificName = await _localizationService.GetResourceAsync("admin.plant.fields.scientificname"),
            Family         = await _localizationService.GetResourceAsync("admin.plant.fields.family"),
            Category       = await _localizationService.GetResourceAsync("admin.plant.fields.category"),
            Flags          = await _localizationService.GetResourceAsync("admin.plant.grid.flags"),
            Actions        = await _localizationService.GetResourceAsync("common.actions"),
        };

        searchModel.GridModel = PlantGridDefinition.Build(searchModel, titles);
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

        var listModel = new PlantListModel();
        return listModel.PrepareToGrid(searchModel, plants, () =>
            plants.Select(entity => _mapper.Map<PlantRowModel>(entity)));
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
