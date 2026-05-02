using GropMng.Core.Common.Exceptions;
using GropMng.Core.Domain.Garden.Enums;
using GropMng.Core.Domain.Garden.Plants;
using GropMng.Core.Interfaces.Services.Garden.Locations;
using GropMng.Core.Interfaces.Services.Garden.Plants;
using GropMng.Core.Interfaces.Services.Media;
using GropMng.Core.Interfaces.Services.User;
using GropMng.Web.Models.Garden;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GropMng.Web.Controllers;

/// <summary>
/// Manages owner plant instances (My Garden → My Plants).
/// </summary>
[Authorize]
[Route("my-garden/plants")]
public class PlantInstanceController : Controller
{
    #region Fields

    private readonly IPlantInstanceService _plantInstanceService;
    private readonly IPlantService _plantService;
    private readonly ILocationService _locationService;
    private readonly ICurrentOwnerProvider _currentOwnerProvider;
    private readonly IPictureService _pictureService;

    #endregion

    #region Ctor

    public PlantInstanceController(
        IPlantInstanceService plantInstanceService,
        IPlantService plantService,
        ILocationService locationService,
        ICurrentOwnerProvider currentOwnerProvider,
        IPictureService pictureService)
    {
        _plantInstanceService = plantInstanceService ?? throw new ArgumentNullException(nameof(plantInstanceService));
        _plantService = plantService ?? throw new ArgumentNullException(nameof(plantService));
        _locationService = locationService ?? throw new ArgumentNullException(nameof(locationService));
        _currentOwnerProvider = currentOwnerProvider ?? throw new ArgumentNullException(nameof(currentOwnerProvider));
        _pictureService = pictureService ?? throw new ArgumentNullException(nameof(pictureService));
    }

    #endregion

    #region Public

    [HttpGet("")]
    public async Task<IActionResult> List(int? plantId, int? gardenSpotId, int? locationId, bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);

        var plantInstances = await _plantInstanceService.GetPlantInstancesAsync(
            ownerId, 
            plantId, 
            gardenSpotId, 
            locationId, 
            activeOnly,
            cancellationToken: cancellationToken);

        // Load related data
        var plants = await _plantService.GetPlantsAsync(cancellationToken: cancellationToken);
        var locations = await _locationService.GetLocationsAsync(ownerId, cancellationToken: cancellationToken);
        var gardenSpots = new List<GropMng.Core.Domain.Garden.Locations.GardenSpot>();
        foreach (var location in locations)
        {
            var spots = await _locationService.GetGardenSpotsAsync(location.Id, ownerId, cancellationToken);
            gardenSpots.AddRange(spots);
        }

        var plantMap = plants.ToDictionary(p => p.Id);
        var spotMap = gardenSpots.ToDictionary(s => s.Id);
        var locationMap = locations.ToDictionary(l => l.Id);

        var rows = plantInstances
            .Where(p => plantMap.ContainsKey(p.PlantId) && spotMap.ContainsKey(p.GardenSpotId))
            .Select(p => new PlantInstanceListRowModel
        {
            Id = p.Id,
            PlantName = plantMap[p.PlantId].ScientificName,
            Nickname = p.Nickname,
            GardenSpotName = spotMap[p.GardenSpotId].Name,
            LocationName = locationMap.TryGetValue(spotMap[p.GardenSpotId].LocationId, out var location) ? location.Name : "—",
            ContainerInfo = p.Container != null ? $"{p.Container.ContainerType}" : null,
            HealthStatus = p.HealthStatus,
            AgeYears = p.AgeYears,
            IsActive = p.IsActive
        }).ToList();

        // Load main photo thumbnail URL (500px) for each plant instance row
        foreach (var row in rows)
        {
            var mainPhoto = await _plantInstanceService.GetMainPlantPhotoAsync(row.Id, ownerId, cancellationToken);
            if (mainPhoto != null)
                row.MainImageUrl = await _pictureService.GetPictureUrlAsync(mainPhoto.PictureId, targetSize: 500);
        }

        var filterModel = new PlantInstanceListFilterModel
        {
            PlantId = plantId,
            GardenSpotId = gardenSpotId,
            LocationId = locationId,
            ActiveOnly = activeOnly,
            AvailablePlants = plants.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.ScientificName,
                Selected = p.Id == plantId
            }).ToList(),
            AvailableLocations = locations.Select(l => new SelectListItem
            {
                Value = l.Id.ToString(),
                Text = l.Name,
                Selected = l.Id == locationId
            }).ToList(),
            AvailableGardenSpots = gardenSpots.Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = s.Name,
                Selected = s.Id == gardenSpotId
            }).ToList()
        };

        ViewBag.FilterModel = filterModel;
        return View(rows);
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);
        var model = await PrepareModelAsync(new PlantInstanceModel(), ownerId, cancellationToken);
        return View(model);
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PlantInstanceModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);
            return View(await PrepareModelAsync(model, ownerId, cancellationToken));
        }

        var ownerId2 = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);

        try
        {
            await _plantInstanceService.CreatePlantInstanceAsync(new PlantInstance
            {
                OwnerId = ownerId2,
                PlantId = model.PlantId,
                GardenSpotId = model.GardenSpotId,
                ContainerId = model.ContainerId,
                SoilMixId = model.SoilMixId,
                Nickname = model.Nickname,
                PlantedDate = model.PlantedDate,
                SizeCategory = model.SizeCategory,
                HeightCm = model.HeightCm,
                SpreadCm = model.SpreadCm,
                HealthStatus = model.HealthStatus,
                IsActive = model.IsActive,
                Notes = model.Notes
            }, cancellationToken);

            TempData["SuccessMessage"] = "Plant added successfully.";
            return RedirectToAction(nameof(List));
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(await PrepareModelAsync(model, ownerId2, cancellationToken));
        }
    }

    [HttpGet("{id:int}/edit")]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);
        var instance = await _plantInstanceService.GetPlantInstanceByIdAsync(id, ownerId, includeDetails: true, cancellationToken);

        if (instance is null)
            return RedirectToAction(nameof(List));

        var plants = await _plantService.GetPlantsAsync(cancellationToken: cancellationToken);
        var locations = await _locationService.GetLocationsAsync(ownerId, cancellationToken: cancellationToken);
        var gardenSpots = new List<GropMng.Core.Domain.Garden.Locations.GardenSpot>();
        foreach (var location in locations)
        {
            var spots = await _locationService.GetGardenSpotsAsync(location.Id, ownerId, cancellationToken);
            gardenSpots.AddRange(spots);
        }

        var locationMap = locations.ToDictionary(l => l.Id);
        var gardenSpot = gardenSpots.FirstOrDefault(s => s.Id == instance.GardenSpotId);

        var model = new PlantInstanceModel
        {
            Id = instance.Id,
            PlantId = instance.PlantId,
            PlantName = plants.FirstOrDefault(p => p.Id == instance.PlantId)?.ScientificName ?? "—",
            GardenSpotId = instance.GardenSpotId,
            GardenSpotName = gardenSpot?.Name ?? "—",
            LocationName = gardenSpot != null && locationMap.TryGetValue(gardenSpot.LocationId, out var selectedLocation)
                ? selectedLocation.Name
                : "—",
            ContainerId = instance.ContainerId,
            ContainerInfo = instance.Container?.ContainerType.ToString(),
            SoilMixId = instance.SoilMixId,
            SoilMixName = instance.SoilMix?.Name,
            Nickname = instance.Nickname,
            PlantedDate = instance.PlantedDate,
            AgeYears = instance.AgeYears,
            SizeCategory = instance.SizeCategory,
            HeightCm = instance.HeightCm,
            SpreadCm = instance.SpreadCm,
            HealthStatus = instance.HealthStatus,
            IsActive = instance.IsActive,
            Notes = instance.Notes
        };

        return View(await PrepareModelAsync(model, ownerId, cancellationToken));
    }

    [HttpPost("{id:int}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PlantInstanceModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);
            return View(await PrepareModelAsync(model, ownerId, cancellationToken));
        }

        var ownerId2 = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);
        var instance = await _plantInstanceService.GetPlantInstanceByIdAsync(id, ownerId2, includeDetails: true, cancellationToken);

        if (instance is null)
            return RedirectToAction(nameof(List));

        try
        {
            instance.PlantId = model.PlantId;
            instance.GardenSpotId = model.GardenSpotId;
            instance.ContainerId = model.ContainerId;
            instance.SoilMixId = model.SoilMixId;
            instance.Nickname = model.Nickname;
            instance.PlantedDate = model.PlantedDate;
            instance.SizeCategory = model.SizeCategory;
            instance.HeightCm = model.HeightCm;
            instance.SpreadCm = model.SpreadCm;
            instance.HealthStatus = model.HealthStatus;
            instance.IsActive = model.IsActive;
            instance.Notes = model.Notes;

            await _plantInstanceService.UpdatePlantInstanceAsync(instance, cancellationToken);

            TempData["SuccessMessage"] = "Plant updated successfully.";
            return RedirectToAction(nameof(Edit), new { id });
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(await PrepareModelAsync(model, ownerId2, cancellationToken));
        }
    }

    [HttpPost("{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);

        try
        {
            await _plantInstanceService.DeletePlantInstanceAsync(id, ownerId, cancellationToken);
            TempData["SuccessMessage"] = "Plant deleted successfully.";
        }
        catch (DomainException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(List));
    }

    #endregion

    #region Private

    private async Task<PlantInstanceModel> PrepareModelAsync(PlantInstanceModel model, Guid ownerId, CancellationToken cancellationToken)
    {
        var plants = await _plantService.GetPlantsAsync(cancellationToken: cancellationToken);
        var locations = await _locationService.GetLocationsAsync(ownerId, cancellationToken: cancellationToken);
        var gardenSpots = new List<GropMng.Core.Domain.Garden.Locations.GardenSpot>();
        foreach (var location in locations)
        {
            var spots = await _locationService.GetGardenSpotsAsync(location.Id, ownerId, cancellationToken);
            gardenSpots.AddRange(spots);
        }
        var locationMap = locations.ToDictionary(l => l.Id);

        model.AvailablePlants = plants.Select(p => new SelectListItem
        {
            Value = p.Id.ToString(),
            Text = p.ScientificName,
            Selected = model.PlantId == p.Id
        }).ToList();

        model.AvailableGardenSpots = gardenSpots
            .Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = locationMap.TryGetValue(s.LocationId, out var location) ? $"{location.Name} - {s.Name}" : s.Name,
                Selected = model.GardenSpotId == s.Id
            })
            .ToList();

        var containers = await _plantInstanceService.GetContainersAsync(ownerId, cancellationToken);
        model.AvailableContainers = new List<SelectListItem> { new() { Value = "", Text = "— None —" } }
            .Concat(containers.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.ContainerType} {(c.Color != null ? $"({c.Color})" : "")}",
                Selected = model.ContainerId == c.Id
            }))
            .ToList();

        var soilMixes = await _plantInstanceService.GetSoilMixesAsync(cancellationToken);
        model.AvailableSoilMixes = new List<SelectListItem> { new() { Value = "", Text = "— None —" } }
            .Concat(soilMixes.Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = s.Name,
                Selected = model.SoilMixId == s.Id
            }))
            .ToList();

        model.AvailableSizeCategories = new List<SelectListItem> { new() { Value = "", Text = "— Unspecified —" } }
            .Concat(Enum.GetValues<PlantSizeCategory>().Select(c => new SelectListItem
            {
                Value = c.ToString(),
                Text = c.ToString(),
                Selected = model.SizeCategory == c
            }))
            .ToList();

        model.AvailableHealthStatuses = Enum.GetValues<PlantHealthStatus>().Select(s => new SelectListItem
        {
            Value = s.ToString(),
            Text = s.ToString(),
            Selected = model.HealthStatus == s
        }).ToList();

        return model;
    }

    #endregion

    #region Plant Photo Actions

    [HttpGet("{id:int}/photos")]
    public async Task<IActionResult> PlantPhotoList(int id, CancellationToken cancellationToken)
    {
        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);

        var instance = await _plantInstanceService.GetPlantInstanceByIdAsync(id, ownerId, includeDetails: false, cancellationToken);
        if (instance is null)
            return Json(new { data = Array.Empty<object>() });

        var photos = await _plantInstanceService.GetPlantPhotosAsync(id, ownerId, cancellationToken);

        var rows = new List<PlantInstancePhotoRowModel>();
        foreach (var photo in photos)
        {
            var thumbUrl = await _pictureService.GetPictureUrlAsync(photo.PictureId, targetSize: 100);
            rows.Add(new PlantInstancePhotoRowModel
            {
                Id = photo.Id,
                PictureId = photo.PictureId,
                ThumbnailUrl = thumbUrl,
                Caption = photo.Caption,
                TakenDate = photo.TakenDate.ToString("yyyy-MM-dd"),
                DisplayOrder = photo.DisplayOrder
            });
        }

        return Json(new { data = rows });
    }

    [HttpPost("{id:int}/photos/add")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlantPhotoAdd(int id, PlantInstancePhotoModel model, CancellationToken cancellationToken)
    {
        if (model.PictureId <= 0)
            return Json(new { success = false, message = "Please upload an image first." });

        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);

        try
        {
            var photo = new PlantPhoto
            {
                OwnerId = ownerId,
                PictureId = model.PictureId,
                Caption = model.Caption,
                TakenDate = model.TakenDate,
                DisplayOrder = model.DisplayOrder
            };

            var created = await _plantInstanceService.AddPlantPhotoAsync(id, photo, cancellationToken);
            var thumbUrl = await _pictureService.GetPictureUrlAsync(created.PictureId, targetSize: 100);

            return Json(new
            {
                success = true,
                data = new PlantInstancePhotoRowModel
                {
                    Id = created.Id,
                    PictureId = created.PictureId,
                    ThumbnailUrl = thumbUrl,
                    Caption = created.Caption,
                    TakenDate = created.TakenDate.ToString("yyyy-MM-dd"),
                    DisplayOrder = created.DisplayOrder
                }
            });
        }
        catch (DomainException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("{id:int}/photos/{photoId:int}/update")]
    [HttpPost("{id:int}/photos/update")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlantPhotoUpdate(int id, int photoId, PlantInstancePhotoModel model, CancellationToken cancellationToken)
    {
        var effectivePhotoId = photoId;
        if (effectivePhotoId <= 0 && Request.HasFormContentType)
        {
            var formPhotoId = Request.Form["photoId"].FirstOrDefault();
            if (int.TryParse(formPhotoId, out var parsedPhotoId))
                effectivePhotoId = parsedPhotoId;
        }

        if (effectivePhotoId <= 0)
            return Json(new { success = false, message = "Photo not found." });

        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);

        try
        {
            var existing = await _plantInstanceService.GetPlantPhotoByIdAsync(id, effectivePhotoId, ownerId, cancellationToken);
            if (existing is null)
                return Json(new { success = false, message = "Photo not found." });

            existing.Caption = model.Caption;
            existing.TakenDate = model.TakenDate;
            existing.DisplayOrder = model.DisplayOrder;

            await _plantInstanceService.UpdatePlantPhotoAsync(id, existing, cancellationToken);

            return Json(new { success = true });
        }
        catch (DomainException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("{id:int}/photos/{photoId:int}/delete")]
    [HttpPost("{id:int}/photos/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlantPhotoDelete(int id, int photoId, CancellationToken cancellationToken)
    {
        if (photoId <= 0 && Request.HasFormContentType)
        {
            var formPhotoId = Request.Form["photoId"].FirstOrDefault();
            if (int.TryParse(formPhotoId, out var parsedPhotoId))
                photoId = parsedPhotoId;
        }

        if (photoId <= 0)
            return Json(new { success = false, message = "Photo not found." });

        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);

        try
        {
            await _plantInstanceService.DeletePlantPhotoAsync(id, photoId, ownerId, cancellationToken);
            return Json(new { success = true });
        }
        catch (DomainException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    #endregion
}
