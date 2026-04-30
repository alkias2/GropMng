using GropMng.Core.Domain.Garden.Enums;
using GropMng.Core.Domain.Garden.Health;
using GropMng.Core.Interfaces.Services.Garden.Health;
using GropMng.Web.Areas.Admin.Models.Disease;
using GropMng.Web.Framework.Models.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GropMng.Web.Areas.Admin.Factories.Disease;

/// <summary>
/// Default implementation for Disease admin model preparation and persistence orchestration.
/// </summary>
public class DiseaseModelFactory : IDiseaseModelFactory
{
    #region Fields

    private readonly IDiseaseService _diseaseService;
    private readonly IPesticideService _pesticideService;

    #endregion

    #region Ctor

    public DiseaseModelFactory(IDiseaseService diseaseService, IPesticideService pesticideService)
    {
        _diseaseService = diseaseService;
        _pesticideService = pesticideService;
    }

    #endregion

    #region Public

    /// <inheritdoc />
    public Task<DiseaseSearchModel> PrepareSearchModelAsync(DiseaseSearchModel? searchModel = null, CancellationToken cancellationToken = default)
    {
        searchModel ??= new DiseaseSearchModel();
        searchModel.SetGridPageSize();
        return Task.FromResult(searchModel);
    }

    /// <inheritdoc />
    public async Task<DiseaseListModel> PrepareListModelAsync(DiseaseSearchModel searchModel, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        var pageIndex = searchModel.Page - 1;
        var diseases = await _diseaseService.GetDiseasesAsync(
            searchModel.SearchTerm,
            pageIndex,
            searchModel.PageSize,
            cancellationToken);

        var rows = diseases.Select(d => new DiseaseRowModel
        {
            Id = d.Id,
            Name = d.Name,
            DiseaseTypeLocalized = d.DiseaseType.ToString(),
            AffectedParts = d.AffectedParts
        }).ToList();

        var listModel = new DiseaseListModel();
        return listModel.PrepareToGrid(searchModel, diseases, () => rows);
    }

    /// <inheritdoc />
    public Task<DiseaseModel> PrepareCreateModelAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(PrepareDiseaseTypeLookups(new DiseaseModel()));
    }

    /// <inheritdoc />
    public async Task<DiseaseModel?> PrepareEditModelAsync(int diseaseId, CancellationToken cancellationToken = default)
    {
        var entity = await _diseaseService.GetDiseaseByIdAsync(diseaseId, cancellationToken: cancellationToken);
        if (entity == null)
            return null;

        var model = new DiseaseModel
        {
            Id = entity.Id,
            Name = entity.Name,
            DiseaseType = entity.DiseaseType,
            Symptoms = entity.Symptoms,
            AffectedParts = entity.AffectedParts,
            PreventionNotes = entity.PreventionNotes,
            Notes = entity.Notes
        };

        return PrepareDiseaseTypeLookups(model);
    }

    /// <inheritdoc />
    public async Task SaveCreateAsync(DiseaseModel model, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);

        var entity = new GropMng.Core.Domain.Garden.Health.Disease
        {
            Name = model.Name,
            DiseaseType = model.DiseaseType,
            Symptoms = model.Symptoms,
            AffectedParts = model.AffectedParts,
            PreventionNotes = model.PreventionNotes,
            Notes = model.Notes,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        await _diseaseService.CreateDiseaseAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> SaveEditAsync(DiseaseModel model, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);

        var existing = await _diseaseService.GetDiseaseByIdAsync(model.Id, cancellationToken: cancellationToken);
        if (existing == null)
            return false;

        existing.Name = model.Name;
        existing.DiseaseType = model.DiseaseType;
        existing.Symptoms = model.Symptoms;
        existing.AffectedParts = model.AffectedParts;
        existing.PreventionNotes = model.PreventionNotes;
        existing.Notes = model.Notes;
        existing.UpdatedAtUtc = DateTime.UtcNow;

        await _diseaseService.UpdateDiseaseAsync(existing, cancellationToken);
        return true;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<DiseaseRemedyLinkRowModel>> PrepareRemedyLinkRowsAsync(int diseaseId, CancellationToken cancellationToken = default)
    {
        var links = await _diseaseService.GetRemedyLinksAsync(diseaseId, cancellationToken);

        return links.Select(l => new DiseaseRemedyLinkRowModel
        {
            Id = l.Id,
            DiseaseId = l.DiseaseId,
            PesticideName = l.Pesticide?.Name ?? string.Empty,
            TreatmentTypeLocalized = l.TreatmentType.ToString(),
            Dosage = l.Dosage,
            Frequency = l.Frequency
        }).ToList();
    }

    /// <inheritdoc />
    public async Task<DiseaseRemedyLinkModel> PrepareRemedyLinkCreateModelAsync(int diseaseId, CancellationToken cancellationToken = default)
    {
        var model = new DiseaseRemedyLinkModel { DiseaseId = diseaseId };
        await PrepareRemedyLinkLookups(model, cancellationToken);
        return model;
    }

    /// <inheritdoc />
    public async Task AddRemedyLinkAsync(DiseaseRemedyLinkModel model, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);

        var link = new DiseaseRemedyLink
        {
            DiseaseId = model.DiseaseId,
            PesticideId = model.PesticideId,
            TreatmentType = model.TreatmentType,
            Dosage = model.Dosage,
            Frequency = model.Frequency,
            Notes = model.Notes,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        await _diseaseService.AddRemedyLinkAsync(model.DiseaseId, link, cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteRemedyLinkAsync(int diseaseId, int remedyLinkId, CancellationToken cancellationToken = default)
    {
        await _diseaseService.DeleteRemedyLinkAsync(diseaseId, remedyLinkId, cancellationToken);
    }

    #endregion

    #region Privates

    private static DiseaseModel PrepareDiseaseTypeLookups(DiseaseModel model)
    {
        model.AvailableDiseaseTypes = Enum.GetValues<PlantDiseaseType>()
            .Select(v => new SelectListItem
            {
                Value = v.ToString(),
                Text = v.ToString(),
                Selected = model.DiseaseType == v
            })
            .ToList();

        return model;
    }

    private async Task PrepareRemedyLinkLookups(DiseaseRemedyLinkModel model, CancellationToken cancellationToken)
    {
        var pesticides = await _pesticideService.GetPesticidesAsync(pageSize: int.MaxValue, cancellationToken: cancellationToken);

        model.AvailablePesticides = pesticides
            .Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Name,
                Selected = model.PesticideId == p.Id
            })
            .ToList();

        model.AvailableTreatmentTypes = Enum.GetValues<RemedyTreatmentType>()
            .Select(v => new SelectListItem
            {
                Value = v.ToString(),
                Text = v.ToString(),
                Selected = model.TreatmentType == v
            })
            .ToList();
    }

    #endregion
}
