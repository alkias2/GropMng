using GropMng.Core.Domain.Garden.Enums;
using GropMng.Core.Domain.Garden.Plants;
using GropMng.Core.Interfaces.Services.Garden.Plants;
using GropMng.Web.Areas.Admin.Models.SoilMix;
using GropMng.Web.Framework.Models.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using GropMng.Core.Common.Exceptions;

namespace GropMng.Web.Areas.Admin.Factories.SoilMix;

/// <summary>
/// Default implementation for SoilMix admin model preparation and persistence orchestration.
/// </summary>
public class SoilMixModelFactory : ISoilMixModelFactory
{
    private readonly ISoilMixService _soilMixService;

    public SoilMixModelFactory(ISoilMixService soilMixService)
    {
        _soilMixService = soilMixService;
    }

    public Task<SoilMixSearchModel> PrepareSearchModelAsync(SoilMixSearchModel? searchModel = null, CancellationToken cancellationToken = default)
    {
        searchModel ??= new SoilMixSearchModel();
        searchModel.SetGridPageSize();
        return Task.FromResult(searchModel);
    }

    public async Task<SoilMixListModel> PrepareListModelAsync(SoilMixSearchModel searchModel, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        var pageIndex = searchModel.Page - 1;
        var soilMixes = await _soilMixService.GetSoilMixesAsync(searchModel.SearchTerm, pageIndex, searchModel.PageSize, cancellationToken);

        var rows = soilMixes.Select(m => new SoilMixRowModel
        {
            Id = m.Id,
            Name = m.Name,
            TextureLocalized = m.Texture?.ToString(),
            DrainageLocalized = m.Drainage?.ToString(),
            PhRange = BuildPhRange(m.PhMin, m.PhMax)
        }).ToList();

        var listModel = new SoilMixListModel();
        return listModel.PrepareToGrid(searchModel, soilMixes, () => rows);
    }

    public Task<SoilMixModel> PrepareCreateModelAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(PrepareLookups(new SoilMixModel()));
    }

    public async Task<SoilMixModel?> PrepareEditModelAsync(int soilMixId, CancellationToken cancellationToken = default)
    {
        var entity = await _soilMixService.GetSoilMixByIdAsync(soilMixId, cancellationToken);
        if (entity == null)
            return null;

        var model = new SoilMixModel
        {
            Id = entity.Id,
            Name = entity.Name,
            Composition = entity.Composition,
            PhMin = entity.PhMin,
            PhMax = entity.PhMax,
            Texture = entity.Texture,
            Drainage = entity.Drainage,
            Notes = entity.Notes
        };

        return PrepareLookups(model);
    }

    public async Task SaveCreateAsync(SoilMixModel model, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);

        var entity = new GropMng.Core.Domain.Garden.Plants.SoilMix
        {
            Name = model.Name,
            Composition = model.Composition,
            PhMin = model.PhMin,
            PhMax = model.PhMax,
            Texture = model.Texture,
            Drainage = model.Drainage,
            Notes = model.Notes,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        await _soilMixService.CreateSoilMixAsync(entity, cancellationToken);
    }

    public async Task<bool> SaveEditAsync(SoilMixModel model, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);

        var existing = await _soilMixService.GetSoilMixByIdAsync(model.Id, cancellationToken);
        if (existing == null)
            return false;

        existing.Name = model.Name;
        existing.Composition = model.Composition;
        existing.PhMin = model.PhMin;
        existing.PhMax = model.PhMax;
        existing.Texture = model.Texture;
        existing.Drainage = model.Drainage;
        existing.Notes = model.Notes;
        existing.UpdatedAtUtc = DateTime.UtcNow;

        await _soilMixService.UpdateSoilMixAsync(existing, cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<SoilMixIngredientRowModel>> PrepareIngredientRowsAsync(int soilMixId, CancellationToken cancellationToken = default)
    {
        var ingredients = await _soilMixService.GetSoilMixIngredientsAsync(soilMixId, cancellationToken);

        return ingredients.Select(i => new SoilMixIngredientRowModel
        {
            Id = i.Id,
            SoilMixId = i.SoilMixId,
            SoilIngredientName = i.SoilIngredient?.Name ?? string.Empty,
            PercentageByVolume = i.PercentageByVolume,
            Notes = i.Notes
        }).ToList();
    }

    public async Task<SoilMixIngredientModel> PrepareIngredientCreateModelAsync(int soilMixId, CancellationToken cancellationToken = default)
    {
        var model = new SoilMixIngredientModel { SoilMixId = soilMixId };
        await PrepareIngredientLookupsAsync(model, cancellationToken);
        return model;
    }

    public async Task AddIngredientAsync(SoilMixIngredientModel model, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);

        int soilIngredientId;

        if (model.IsNew)
        {
            if (string.IsNullOrWhiteSpace(model.NewIngredientName))
                throw new DomainException("Ingredient name is required when creating a new ingredient.");

            var newIngredient = new SoilIngredient
            {
                Name = model.NewIngredientName.Trim(),
                Description = model.NewIngredientDescription?.Trim()
            };

            var created = await _soilMixService.CreateSoilIngredientAsync(newIngredient, cancellationToken);
            soilIngredientId = created.Id;
        }
        else
        {
            if (model.SoilIngredientId <= 0)
                throw new DomainException("Please select an existing ingredient.");

            soilIngredientId = model.SoilIngredientId;
        }

        var ingredient = new SoilMixIngredient
        {
            SoilMixId = model.SoilMixId,
            SoilIngredientId = soilIngredientId,
            PercentageByVolume = model.PercentageByVolume,
            Notes = model.Notes
        };

        await _soilMixService.AddSoilMixIngredientAsync(model.SoilMixId, ingredient, cancellationToken);
    }

    public Task UpdateIngredientAsync(int soilMixId, int id, decimal percentageByVolume, string? notes, CancellationToken cancellationToken = default)
    {
        return _soilMixService.UpdateSoilMixIngredientAsync(soilMixId, id, percentageByVolume, notes, cancellationToken);
    }

    public Task DeleteIngredientAsync(int soilMixId, int soilMixIngredientId, CancellationToken cancellationToken = default)
    {
        return _soilMixService.DeleteSoilMixIngredientAsync(soilMixId, soilMixIngredientId, cancellationToken);
    }

    private static SoilMixModel PrepareLookups(SoilMixModel model)
    {
        model.AvailableTextures = BuildNullableEnumList<SoilTextureType>();
        model.AvailableDrainages = BuildNullableEnumList<SoilDrainageType>();
        return model;
    }

    private async Task PrepareIngredientLookupsAsync(SoilMixIngredientModel model, CancellationToken cancellationToken)
    {
        var allIngredients = await _soilMixService.GetSoilIngredientsAsync(cancellationToken);
        var usedIds = model.SoilMixId > 0
            ? await _soilMixService.GetUsedSoilIngredientIdsAsync(model.SoilMixId, cancellationToken)
            : Array.Empty<int>();

        model.AvailableSoilIngredients = allIngredients
            .Where(i => !usedIds.Contains(i.Id))
            .Select(i => new SelectListItem
            {
                Value = i.Id.ToString(),
                Text = i.Name,
                Selected = model.SoilIngredientId == i.Id
            }).ToList();
    }

    private static IList<SelectListItem> BuildNullableEnumList<TEnum>() where TEnum : struct, Enum
    {
        var items = new List<SelectListItem> { new() { Value = string.Empty, Text = "-- None --" } };
        items.AddRange(Enum.GetValues<TEnum>().Select(v => new SelectListItem { Value = v.ToString(), Text = v.ToString() }));
        return items;
    }

    private static string? BuildPhRange(decimal? phMin, decimal? phMax)
    {
        if (!phMin.HasValue && !phMax.HasValue)
            return null;

        if (phMin.HasValue && phMax.HasValue)
            return $"{phMin:0.##} - {phMax:0.##}";

        return phMin.HasValue ? $"{phMin:0.##}+" : $"<= {phMax:0.##}";
    }
}
