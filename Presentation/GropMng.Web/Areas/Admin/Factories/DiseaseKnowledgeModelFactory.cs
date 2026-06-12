using GropMng.Core.Domain.Garden.Enums;
using GropMng.Core.Domain.Garden.Health;
using GropMng.Core.Interfaces.Repositories;
using GropMng.Core.Interfaces.Services.Garden.Health;
using GropMng.Core.Interfaces.Services.Media;
using GropMng.Web.Areas.Admin.Models;
using GropMng.Web.Framework.Models.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GropMng.Web.Areas.Admin.Factories;

/// <summary>
/// Default implementation for DiseaseKnowledge admin model preparation and persistence orchestration.
/// </summary>
public class DiseaseKnowledgeModelFactory : IDiseaseKnowledgeModelFactory
{
    #region Fields

    private readonly IDiseaseKnowledgeService _diseaseKnowledgeService;
    private readonly IAdminNotificationService _notificationService;
    private readonly IRepository<GropMng.Core.Domain.Garden.Plants.Plant> _plantRepository;
    private readonly IRepository<DiseaseKnowledgePhoto> _photoRepository;
    private readonly IRepository<DiseaseKnowledgePlant> _plantLinkRepository;
    private readonly IPictureService _pictureService;

    #endregion

    #region Ctor

    public DiseaseKnowledgeModelFactory(
        IDiseaseKnowledgeService diseaseKnowledgeService,
        IAdminNotificationService notificationService,
        IRepository<GropMng.Core.Domain.Garden.Plants.Plant> plantRepository,
        IRepository<DiseaseKnowledgePhoto> photoRepository,
        IRepository<DiseaseKnowledgePlant> plantLinkRepository,
        IPictureService pictureService)
    {
        _diseaseKnowledgeService = diseaseKnowledgeService;
        _notificationService = notificationService;
        _plantRepository = plantRepository;
        _photoRepository = photoRepository;
        _plantLinkRepository = plantLinkRepository;
        _pictureService = pictureService;
    }

    #endregion

    #region Public

    /// <inheritdoc />
    public async Task<List<DiseaseKnowledgeListModel>> PrepareListModelAsync(CancellationToken cancellationToken = default)
    {
        var entries = await _diseaseKnowledgeService.GetAllAsync(cancellationToken);

        // Populate plant link counts and photo counts for each entry
        // GroupBy in-memory: EF Core cannot reliably translate GroupBy + First to SQL
        var knowledgeIds = entries.Select(e => e.Id).ToHashSet();

        var photos = await _photoRepository.GetAllAsync(
            query => query.Where(p => knowledgeIds.Contains(p.DiseaseKnowledgeId) && !p.IsDeleted),
            cancellationToken: cancellationToken);

        var photoCountByKnowledgeId = photos
            .GroupBy(p => p.DiseaseKnowledgeId)
            .ToDictionary(g => g.Key, g => g.Count());

        // Load plant link counts via the plant-link repository because the navigation property
        // (PlantLinks) is not eagerly loaded by GetAllAsync. Counting via the repository
        // guarantees the correct value regardless of lazy-loading configuration.
        var plantLinks = await _plantLinkRepository.GetAllAsync(
            query => query.Where(p => knowledgeIds.Contains(p.DiseaseKnowledgeId) && !p.IsDeleted),
            cancellationToken: cancellationToken);

        var linkCountByKnowledgeId = plantLinks
            .GroupBy(p => p.DiseaseKnowledgeId)
            .ToDictionary(g => g.Key, g => g.Count());

        return entries.Select(e => new DiseaseKnowledgeListModel
        {
            Id = e.Id,
            CommonName = e.CommonName,
            ScientificName = e.ScientificName,
            LinkedPlantCount = linkCountByKnowledgeId.TryGetValue(e.Id, out var linkCount) ? linkCount : 0,
            PhotoCount = photoCountByKnowledgeId.TryGetValue(e.Id, out var photoCount) ? photoCount : 0,
            CreatedAtUtc = e.CreatedAtUtc
        }).ToList();
    }

    /// <inheritdoc />
    public async Task<DiseaseKnowledgeEditModel> PrepareCreateModelAsync(
        int? fromNotificationId = null,
        CancellationToken cancellationToken = default)
    {
        var selectedIds = new List<int>();
        var model = new DiseaseKnowledgeEditModel
        {
            SelectedPlantIds = selectedIds,
            AvailablePlants = await BuildPlantMultiSelectListAsync(selectedIds, cancellationToken),
            FromNotificationId = fromNotificationId
        };

        // Pre-fill common name from the admin notification
        if (fromNotificationId.HasValue)
        {
            try
            {
                var notifications = await _notificationService.GetPendingAsync(cancellationToken);
                var notification = notifications.FirstOrDefault(n => n.Id == fromNotificationId.Value);
                if (notification != null)
                {
                    model.CommonName = notification.ProblemName;
                }
            }
            catch
            {
                // Notification may have been resolved already — proceed with empty form
            }
        }

        return model;
    }

    /// <inheritdoc />
    public async Task<DiseaseKnowledgeEditModel?> PrepareEditModelAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        DiseaseKnowledge entity;
        try
        {
            entity = await _diseaseKnowledgeService.GetByIdAsync(id, cancellationToken);
        }
        catch
        {
            return null;
        }

        // Load existing photos
        var photos = await _photoRepository.GetAllAsync(
            query => query.Where(p => p.DiseaseKnowledgeId == id && !p.IsDeleted)
                .OrderBy(p => p.DisplayOrder),
            cancellationToken: cancellationToken);

        var photoModels = new List<DiseaseKnowledgePhotoModel>();
        foreach (var photo in photos)
        {
            var url = await _pictureService.GetPictureUrlAsync(photo.PictureId, targetSize: 200);
            photoModels.Add(new DiseaseKnowledgePhotoModel
            {
                Id = photo.Id,
                PictureId = photo.PictureId,
                PictureUrl = url,
                DisplayOrder = photo.DisplayOrder,
                Caption = photo.Caption
            });
        }

        // Load linked plant IDs from repository (navigation property is not included by GetByIdAsync)
        var links = await _plantLinkRepository.GetAllAsync(
            query => query.Where(p => p.DiseaseKnowledgeId == id && !p.IsDeleted),
            cancellationToken: cancellationToken);

        var selectedPlantIds = links.Select(p => p.PlantId).ToList();

        return new DiseaseKnowledgeEditModel
        {
            Id = entity.Id,
            CommonName = entity.CommonName,
            ScientificName = entity.ScientificName,
            Description = entity.Description,
            TreatmentGuidelines = entity.TreatmentGuidelines,
            SelectedPlantIds = selectedPlantIds,
            AvailablePlants = await BuildPlantMultiSelectListAsync(selectedPlantIds, cancellationToken),
            ExistingPhotos = photoModels
        };
    }

    /// <inheritdoc />
    public async Task<int> SaveCreateAsync(
        DiseaseKnowledgeEditModel model,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);

        var entity = new DiseaseKnowledge
        {
            CommonName = model.CommonName.Trim(),
            ScientificName = model.ScientificName?.Trim(),
            Description = model.Description,
            TreatmentGuidelines = model.TreatmentGuidelines,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
            IsDeleted = false
        };

        var created = await _diseaseKnowledgeService.CreateAsync(entity, cancellationToken);

        // Save plant links
        await SavePlantLinksAsync(created.Id, model.SelectedPlantIds, cancellationToken);

        // Handle photo uploads
        if (model.NewPhotos is { Count: > 0 })
        {
            foreach (var file in model.NewPhotos)
            {
                if (file.Length == 0) continue;

                // Upload picture and get its ID back
                using var ms = new System.IO.MemoryStream();
                await file.CopyToAsync(ms, cancellationToken);
                var pictureBytes = ms.ToArray();
                var picture = await _pictureService.InsertPictureAsync(
                    pictureBytes,
                    file.ContentType,
                    file.FileName,
                    subfolder: "DiseaseKnowledge");

                var photo = new DiseaseKnowledgePhoto
                {
                    DiseaseKnowledgeId = created.Id,
                    PictureId = picture.Id,
                    DisplayOrder = 0,
                    CreatedAtUtc = DateTime.UtcNow,
                    UpdatedAtUtc = DateTime.UtcNow,
                    IsDeleted = false
                };

                await _diseaseKnowledgeService.AddPhotoAsync(photo, cancellationToken);
            }
        }

        return created.Id;
    }

    /// <inheritdoc />
    public async Task<bool> SaveEditAsync(
        DiseaseKnowledgeEditModel model,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);

        var existing = await _diseaseKnowledgeService.GetByIdAsync(model.Id!.Value, cancellationToken);

        existing.CommonName = model.CommonName.Trim();
        existing.ScientificName = model.ScientificName?.Trim();
        existing.Description = model.Description;
        existing.TreatmentGuidelines = model.TreatmentGuidelines;

        var updated = await _diseaseKnowledgeService.UpdateAsync(existing, cancellationToken);

        // Update plant links
        await SavePlantLinksAsync(updated.Id, model.SelectedPlantIds, cancellationToken);

        // Handle new photo uploads
        if (model.NewPhotos is { Count: > 0 })
        {
            foreach (var file in model.NewPhotos)
            {
                if (file.Length == 0) continue;

                using var ms = new System.IO.MemoryStream();
                await file.CopyToAsync(ms, cancellationToken);
                var pictureBytes = ms.ToArray();
                var picture = await _pictureService.InsertPictureAsync(
                    pictureBytes,
                    file.ContentType,
                    file.FileName,
                    subfolder: "DiseaseKnowledge");

                var photo = new DiseaseKnowledgePhoto
                {
                    DiseaseKnowledgeId = updated.Id,
                    PictureId = picture.Id,
                    DisplayOrder = 0,
                    CreatedAtUtc = DateTime.UtcNow,
                    UpdatedAtUtc = DateTime.UtcNow,
                    IsDeleted = false
                };

                await _diseaseKnowledgeService.AddPhotoAsync(photo, cancellationToken);
            }
        }

        return true;
    }

    #endregion

    #region Privates

    /// <summary>
    /// Replaces the plant links for a knowledge entry: soft-deletes removed links, adds new ones.
    /// </summary>
    private async Task SavePlantLinksAsync(int knowledgeId, List<int> selectedPlantIds, CancellationToken cancellationToken)
    {
        // Load existing links
        var existingLinks = await _plantLinkRepository.GetAllAsync(
            query => query.Where(p => p.DiseaseKnowledgeId == knowledgeId && !p.IsDeleted),
            cancellationToken: cancellationToken);

        var existingPlantIds = existingLinks.Select(l => l.PlantId).ToHashSet();
        var selectedSet = selectedPlantIds.ToHashSet();

        var now = DateTime.UtcNow;

        // Soft-delete links that are no longer selected
        foreach (var link in existingLinks)
        {
            if (!selectedSet.Contains(link.PlantId))
            {
                link.IsDeleted = true;
                link.DeletedAtUtc = now;
                link.UpdatedAtUtc = now;
                await _plantLinkRepository.UpdateAsync(link, cancellationToken: cancellationToken);
            }
        }

        // Add newly selected plants
        foreach (var plantId in selectedPlantIds)
        {
            if (!existingPlantIds.Contains(plantId))
            {
                var newLink = new DiseaseKnowledgePlant
                {
                    DiseaseKnowledgeId = knowledgeId,
                    PlantId = plantId,
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now,
                    IsDeleted = false
                };
                await _plantLinkRepository.CreateAsync(newLink, cancellationToken: cancellationToken);
            }
        }
    }

    private async Task<MultiSelectList> BuildPlantMultiSelectListAsync(List<int> selectedPlantIds, CancellationToken cancellationToken)
    {
        var plants = await _plantRepository.GetAllAsync(
            query => query.Where(p => !p.IsDeleted).OrderBy(p => p.ScientificName),
            cancellationToken: cancellationToken);

        var items = plants
            .Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.ScientificName
            })
            .ToList();

        // Pass selected values via the MultiSelectList constructor that supports them
        var selectedValues = selectedPlantIds.Select(id => id.ToString()).ToList();
        return new MultiSelectList(items, "Value", "Text", selectedValues);
    }

    #endregion
}