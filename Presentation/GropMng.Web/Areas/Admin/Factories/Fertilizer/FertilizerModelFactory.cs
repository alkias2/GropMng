using GropMng.Core.Domain.Garden.Care;
using GropMng.Core.Domain.Garden.Enums;
using GropMng.Core.Interfaces.Services.Garden.Care;
using GropMng.Web.Areas.Admin.Models.Fertilizer;
using GropMng.Web.Framework.Models.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GropMng.Web.Areas.Admin.Factories.Fertilizer;

/// <summary>
/// Default implementation for Fertilizer admin model preparation and persistence orchestration.
/// </summary>
public class FertilizerModelFactory : IFertilizerModelFactory
{
    #region Fields

    private readonly IFertilizerService _fertilizerService;

    #endregion

    #region Ctor

    public FertilizerModelFactory(IFertilizerService fertilizerService)
    {
        _fertilizerService = fertilizerService;
    }

    #endregion

    #region Public

    /// <inheritdoc />
    public Task<FertilizerSearchModel> PrepareSearchModelAsync(FertilizerSearchModel? searchModel = null, CancellationToken cancellationToken = default)
    {
        searchModel ??= new FertilizerSearchModel();
        searchModel.SetGridPageSize();
        return Task.FromResult(searchModel);
    }

    /// <inheritdoc />
    public async Task<FertilizerListModel> PrepareListModelAsync(FertilizerSearchModel searchModel, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        var pageIndex = searchModel.Page - 1;
        var fertilizers = await _fertilizerService.GetFertilizersAsync(
            searchModel.SearchTerm,
            pageIndex,
            searchModel.PageSize,
            cancellationToken);

        var rows = fertilizers.Select(f => new FertilizerRowModel
        {
            Id = f.Id,
            Name = f.Name,
            Brand = f.Brand,
            NpkRatio = f.NpkRatio,
            IsOrganic = f.IsOrganic,
            FertilizerTypeLocalized = f.FertilizerType?.ToString(),
            ApplicationMethodLocalized = f.ApplicationMethod?.ToString()
        }).ToList();

        var listModel = new FertilizerListModel();
        return listModel.PrepareToGrid(searchModel, fertilizers, () => rows);
    }

    /// <inheritdoc />
    public Task<FertilizerModel> PrepareCreateModelAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(PrepareLookups(new FertilizerModel()));
    }

    /// <inheritdoc />
    public async Task<FertilizerModel?> PrepareEditModelAsync(int fertilizerId, CancellationToken cancellationToken = default)
    {
        var entity = await _fertilizerService.GetFertilizerByIdAsync(fertilizerId, cancellationToken);
        if (entity == null)
            return null;

        var model = new FertilizerModel
        {
            Id = entity.Id,
            Name = entity.Name,
            Brand = entity.Brand,
            FertilizerType = entity.FertilizerType,
            NpkRatio = entity.NpkRatio,
            ApplicationMethod = entity.ApplicationMethod,
            IsOrganic = entity.IsOrganic,
            Notes = entity.Notes
        };

        return PrepareLookups(model);
    }

    /// <inheritdoc />
    public async Task SaveCreateAsync(FertilizerModel model, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);

        var entity = new GropMng.Core.Domain.Garden.Care.Fertilizer
        {
            Name = model.Name,
            Brand = model.Brand,
            FertilizerType = model.FertilizerType,
            NpkRatio = model.NpkRatio,
            ApplicationMethod = model.ApplicationMethod,
            IsOrganic = model.IsOrganic,
            Notes = model.Notes,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        await _fertilizerService.CreateFertilizerAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> SaveEditAsync(FertilizerModel model, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);

        var existing = await _fertilizerService.GetFertilizerByIdAsync(model.Id, cancellationToken);
        if (existing == null)
            return false;

        existing.Name = model.Name;
        existing.Brand = model.Brand;
        existing.FertilizerType = model.FertilizerType;
        existing.NpkRatio = model.NpkRatio;
        existing.ApplicationMethod = model.ApplicationMethod;
        existing.IsOrganic = model.IsOrganic;
        existing.Notes = model.Notes;
        existing.UpdatedAtUtc = DateTime.UtcNow;

        await _fertilizerService.UpdateFertilizerAsync(existing, cancellationToken);
        return true;
    }

    #endregion

    #region Privates

    private static FertilizerModel PrepareLookups(FertilizerModel model)
    {
        model.AvailableFertilizerTypes = BuildNullableEnumList<FertilizerKind>();
        model.AvailableApplicationMethods = BuildNullableEnumList<FertilizerApplicationMethod>();
        return model;
    }

    private static IList<SelectListItem> BuildNullableEnumList<TEnum>() where TEnum : struct, Enum
    {
        var items = new List<SelectListItem> { new() { Value = string.Empty, Text = "-- None --" } };
        items.AddRange(Enum.GetValues<TEnum>().Select(v => new SelectListItem { Value = v.ToString(), Text = v.ToString() }));
        return items;
    }

    #endregion
}
