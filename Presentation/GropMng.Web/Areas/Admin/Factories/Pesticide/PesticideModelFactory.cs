using GropMng.Core.Domain.Garden.Enums;
using GropMng.Core.Domain.Garden.Health;
using GropMng.Core.Interfaces.Services.Garden.Health;
using GropMng.Web.Areas.Admin.Models.Pesticide;
using GropMng.Web.Framework.Models.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GropMng.Web.Areas.Admin.Factories.Pesticide;

/// <summary>
/// Default implementation for Pesticide admin model preparation and persistence orchestration.
/// </summary>
public class PesticideModelFactory : IPesticideModelFactory
{
    #region Fields

    private readonly IPesticideService _pesticideService;

    #endregion

    #region Ctor

    public PesticideModelFactory(IPesticideService pesticideService)
    {
        _pesticideService = pesticideService;
    }

    #endregion

    #region Public

    /// <inheritdoc />
    public Task<PesticideSearchModel> PrepareSearchModelAsync(PesticideSearchModel? searchModel = null, CancellationToken cancellationToken = default)
    {
        searchModel ??= new PesticideSearchModel();
        searchModel.SetGridPageSize();
        return Task.FromResult(searchModel);
    }

    /// <inheritdoc />
    public async Task<PesticideListModel> PrepareListModelAsync(PesticideSearchModel searchModel, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        var pageIndex = searchModel.Page - 1;
        var pesticides = await _pesticideService.GetPesticidesAsync(
            searchModel.SearchTerm,
            pageIndex,
            searchModel.PageSize,
            cancellationToken);

        var rows = pesticides.Select(p => new PesticideRowModel
        {
            Id = p.Id,
            Name = p.Name,
            Brand = p.Brand,
            ActiveIngredient = p.ActiveIngredient,
            IsOrganic = p.IsOrganic,
            PesticideTypeLocalized = p.PesticideType?.ToString(),
            ApplicationMethodLocalized = p.ApplicationMethod?.ToString()
        }).ToList();

        var listModel = new PesticideListModel();
        return listModel.PrepareToGrid(searchModel, pesticides, () => rows);
    }

    /// <inheritdoc />
    public Task<PesticideModel> PrepareCreateModelAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(PrepareLookups(new PesticideModel()));
    }

    /// <inheritdoc />
    public async Task<PesticideModel?> PrepareEditModelAsync(int pesticideId, CancellationToken cancellationToken = default)
    {
        var entity = await _pesticideService.GetPesticideByIdAsync(pesticideId, cancellationToken: cancellationToken);
        if (entity == null)
            return null;

        var model = new PesticideModel
        {
            Id = entity.Id,
            Name = entity.Name,
            Brand = entity.Brand,
            ActiveIngredient = entity.ActiveIngredient,
            PesticideType = entity.PesticideType,
            ApplicationMethod = entity.ApplicationMethod,
            IsOrganic = entity.IsOrganic,
            WithholdingDays = entity.WithholdingDays,
            SafetyNotes = entity.SafetyNotes,
            Notes = entity.Notes
        };

        return PrepareLookups(model);
    }

    /// <inheritdoc />
    public async Task SaveCreateAsync(PesticideModel model, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);

        var entity = new Core.Domain.Garden.Health.Pesticide
        {
            Name = model.Name,
            Brand = model.Brand,
            ActiveIngredient = model.ActiveIngredient,
            PesticideType = model.PesticideType,
            ApplicationMethod = model.ApplicationMethod,
            IsOrganic = model.IsOrganic,
            WithholdingDays = model.WithholdingDays,
            SafetyNotes = model.SafetyNotes,
            Notes = model.Notes,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        await _pesticideService.CreatePesticideAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> SaveEditAsync(PesticideModel model, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);

        var existing = await _pesticideService.GetPesticideByIdAsync(model.Id, cancellationToken: cancellationToken);
        if (existing == null)
            return false;

        existing.Name = model.Name;
        existing.Brand = model.Brand;
        existing.ActiveIngredient = model.ActiveIngredient;
        existing.PesticideType = model.PesticideType;
        existing.ApplicationMethod = model.ApplicationMethod;
        existing.IsOrganic = model.IsOrganic;
        existing.WithholdingDays = model.WithholdingDays;
        existing.SafetyNotes = model.SafetyNotes;
        existing.Notes = model.Notes;
        existing.UpdatedAtUtc = DateTime.UtcNow;

        await _pesticideService.UpdatePesticideAsync(existing, cancellationToken);
        return true;
    }

    #endregion

    #region Privates

    private static PesticideModel PrepareLookups(PesticideModel model)
    {
        model.AvailablePesticideTypes = BuildNullableEnumList<PesticideKind>();
        model.AvailableApplicationMethods = BuildNullableEnumList<PesticideApplicationMethod>();
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
