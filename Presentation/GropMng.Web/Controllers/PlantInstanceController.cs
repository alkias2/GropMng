using System.Globalization;
using GropMng.Core.Common.Exceptions;
using GropMng.Core.Domain.Garden.Care;
using GropMng.Core.Domain.Garden.Enums;
using GropMng.Core.Domain.Garden.Plants;
using GropMng.Core.Interfaces.Services.Garden.Care;
using GropMng.Core.Interfaces.Services.Garden.Locations;
using GropMng.Core.Interfaces.Services.Garden.Plants;
using GropMng.Core.Interfaces.Services.Localization;
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
    private readonly IFertilizerService _fertilizerService;
    private readonly IEnumLocalizationHelper _enumLocalizationHelper;
    private readonly ICurrentOwnerProvider _currentOwnerProvider;
    private readonly IPictureService _pictureService;
    private readonly IContainerService _containerService;
    private readonly ISoilMixService _soilMixService;
    private readonly IWateringService _wateringService;
    private readonly IFertilizingService _fertilizingService;
    private readonly IPlantPhotoService _plantPhotoService;
    private readonly IPlantNoteService _plantNoteService;
    private readonly IRepottingLogService _repottingLogService;

    #endregion

    #region Ctor

    public PlantInstanceController(
        IPlantInstanceService plantInstanceService,
        IPlantService plantService,
        ILocationService locationService,
        IFertilizerService fertilizerService,
        IEnumLocalizationHelper enumLocalizationHelper,
        ICurrentOwnerProvider currentOwnerProvider,
        IPictureService pictureService,
        IContainerService containerService,
        ISoilMixService soilMixService,
        IWateringService wateringService,
        IFertilizingService fertilizingService,
        IPlantPhotoService plantPhotoService,
        IPlantNoteService plantNoteService,
        IRepottingLogService repottingLogService)
    {
        _plantInstanceService = plantInstanceService ?? throw new ArgumentNullException(nameof(plantInstanceService));
        _plantService = plantService ?? throw new ArgumentNullException(nameof(plantService));
        _locationService = locationService ?? throw new ArgumentNullException(nameof(locationService));
        _fertilizerService = fertilizerService ?? throw new ArgumentNullException(nameof(fertilizerService));
        _enumLocalizationHelper = enumLocalizationHelper ?? throw new ArgumentNullException(nameof(enumLocalizationHelper));
        _currentOwnerProvider = currentOwnerProvider ?? throw new ArgumentNullException(nameof(currentOwnerProvider));
        _pictureService = pictureService ?? throw new ArgumentNullException(nameof(pictureService));
        _containerService = containerService ?? throw new ArgumentNullException(nameof(containerService));
        _soilMixService = soilMixService ?? throw new ArgumentNullException(nameof(soilMixService));
        _wateringService = wateringService ?? throw new ArgumentNullException(nameof(wateringService));
        _fertilizingService = fertilizingService ?? throw new ArgumentNullException(nameof(fertilizingService));
        _plantPhotoService = plantPhotoService ?? throw new ArgumentNullException(nameof(plantPhotoService));
        _plantNoteService = plantNoteService ?? throw new ArgumentNullException(nameof(plantNoteService));
        _repottingLogService = repottingLogService ?? throw new ArgumentNullException(nameof(repottingLogService));
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

        var rows = new List<PlantInstanceListRowModel>();
        foreach (var p in plantInstances.Where(p => plantMap.ContainsKey(p.PlantId) && spotMap.ContainsKey(p.GardenSpotId)))
        {
            rows.Add(new PlantInstanceListRowModel
            {
                Id = p.Id,
                PlantName = plantMap[p.PlantId].ScientificName,
                Nickname = p.Nickname,
                GardenSpotName = spotMap[p.GardenSpotId].Name,
                LocationName = locationMap.TryGetValue(spotMap[p.GardenSpotId].LocationId, out var location) ? location.Name : "—",
                ContainerInfo = p.Container is not null ? await BuildContainerDisplayNameAsync(p.Container) : null,
                HealthStatus = p.HealthStatus,
                AgeYears = p.AgeYears,
                IsActive = p.IsActive
            });
        }

        // Load main photo thumbnail URL (500px) for each plant instance row
        foreach (var row in rows)
        {
            var mainPhoto = await _plantPhotoService.GetMainPhotoAsync(row.Id, ownerId, cancellationToken);
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
            ContainerId = instance.Container?.Id,
            ContainerInfo = instance.Container is not null ? await BuildContainerDisplayNameAsync(instance.Container) : null,
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

        var containers = await _containerService.GetContainersAsync(ownerId, pageSize: int.MaxValue, cancellationToken: cancellationToken);
        var availableContainers = new List<SelectListItem> { new() { Value = "", Text = "— None —" } };
        foreach (var container in containers)
        {
            availableContainers.Add(new SelectListItem
            {
                Value = container.Id.ToString(),
                Text = await BuildContainerDisplayNameAsync(container),
                Selected = model.ContainerId == container.Id
            });
        }

        model.AvailableContainers = availableContainers;

        var soilMixes = await _soilMixService.GetSoilMixesAsync(pageSize: int.MaxValue, cancellationToken: cancellationToken);
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

        var photos = await _plantPhotoService.GetPhotosAsync(id, ownerId, cancellationToken);

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

            var created = await _plantPhotoService.CreatePhotoAsync(id, photo, cancellationToken);
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
            var existing = await _plantPhotoService.GetPhotoByIdAsync(id, effectivePhotoId, ownerId, cancellationToken);
            if (existing is null)
                return Json(new { success = false, message = "Photo not found." });

            existing.Caption = model.Caption;
            existing.TakenDate = model.TakenDate;
            existing.DisplayOrder = model.DisplayOrder;

            await _plantPhotoService.UpdatePhotoAsync(id, existing, cancellationToken);

            return Json(new { success = true });
        }
        catch (DomainException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

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
            await _plantPhotoService.DeletePhotoAsync(id, photoId, ownerId, cancellationToken);
            return Json(new { success = true });
        }
        catch (DomainException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    #endregion

    #region Watering Schedule Actions

    [HttpGet("{id:int}/watering")]
    public async Task<IActionResult> WateringScheduleTab(int id, CancellationToken cancellationToken)
    {
        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);
        var instance = await _plantInstanceService.GetPlantInstanceByIdAsync(id, ownerId, includeDetails: false, cancellationToken);
        if (instance is null)
            return NotFound();

        var schedules = await _wateringService.GetSchedulesAsync(id, ownerId, cancellationToken);
        var recentLogs = await _wateringService.GetLogsAsync(id, ownerId, pageIndex: 0, pageSize: 5, cancellationToken);

        var rows = schedules.Select(s => new WateringScheduleRowModel
        {
            Id = s.Id,
            Season = s.Season,
            SeasonKey = s.Season.ToString().ToLowerInvariant(),
            FrequencyLabel = s.FrequencyDays.ToString(),
            WaterAmountLabel = s.WaterAmountL.HasValue ? $"{s.WaterAmountL:0.##} L" : null,
            TimeOfDay = s.TimeOfDay,
            TimeOfDayKey = s.TimeOfDay?.ToString().ToLowerInvariant(),
            Notes = s.Notes
        }).ToList();

        var logRows = recentLogs.Select(l => new WateringLogRowModel
        {
            Id = l.Id,
            WateredAtUtc = l.WateredAtUtc,
            WaterAmountLabel = l.WaterAmountL.HasValue ? $"{l.WaterAmountL:0.##} L" : null,
            Notes = l.Notes
        }).ToList();

        ViewBag.PlantInstanceId = id;
        ViewBag.LogRows = logRows;

        return PartialView("_WateringScheduleTab", rows);
    }

    [HttpPost("{id:int}/watering/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> WateringScheduleCreate(int id, WateringScheduleModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return Json(new { success = false, message = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)) });

        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);

        try
        {
            var schedule = new GropMng.Core.Domain.Garden.Care.WateringSchedule
            {
                OwnerId = ownerId,
                Season = model.Season,
                FrequencyDays = model.FrequencyDays,
                WaterAmountL = model.WaterAmountL,
                TimeOfDay = model.TimeOfDay,
                Notes = model.Notes
            };

            await _wateringService.CreateScheduleAsync(id, schedule, cancellationToken);
            return Json(new { success = true });
        }
        catch (DomainException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("{id:int}/watering/{scheduleId:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> WateringScheduleUpdate(int id, int scheduleId, WateringScheduleModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return Json(new { success = false, message = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)) });

        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);

        try
        {
            var schedule = new GropMng.Core.Domain.Garden.Care.WateringSchedule
            {
                Id = scheduleId,
                OwnerId = ownerId,
                Season = model.Season,
                FrequencyDays = model.FrequencyDays,
                WaterAmountL = model.WaterAmountL,
                TimeOfDay = model.TimeOfDay,
                Notes = model.Notes
            };

            await _wateringService.UpdateScheduleAsync(id, schedule, cancellationToken);
            return Json(new { success = true });
        }
        catch (DomainException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("{id:int}/watering/{scheduleId:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> WateringScheduleDelete(int id, int scheduleId, CancellationToken cancellationToken)
    {
        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);

        try
        {
            await _wateringService.DeleteScheduleAsync(id, scheduleId, ownerId, cancellationToken);
            return Json(new { success = true });
        }
        catch (DomainException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("{id:int}/watering/logs/{logId:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> WateringLogDelete(int id, int logId, CancellationToken cancellationToken)
    {
        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);

        try
        {
            await _wateringService.DeleteLogAsync(id, logId, ownerId, cancellationToken);
            return Json(new { success = true });
        }
        catch (DomainException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    #endregion

    #region Fertilizing Schedule Actions

    [HttpGet("{id:int}/fertilizing")]
    public async Task<IActionResult> FertilizingScheduleTab(int id, CancellationToken cancellationToken)
    {
        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);
        var instance = await _plantInstanceService.GetPlantInstanceByIdAsync(id, ownerId, includeDetails: false, cancellationToken);
        if (instance is null)
            return NotFound();

        var fertilizersPaged = await _fertilizerService.GetFertilizersAsync(pageIndex: 0, pageSize: int.MaxValue, cancellationToken: cancellationToken);
        var fertilizers = fertilizersPaged.ToList();
        var fertilizerMap = fertilizers.ToDictionary(f => f.Id, f => f.Name);

        var schedules = await _fertilizingService.GetSchedulesAsync(id, ownerId, cancellationToken);
        var recentLogs = await _fertilizingService.GetLogsAsync(id, ownerId, pageIndex: 0, pageSize: 5, cancellationToken);

        var rows = schedules.Select(s => new FertilizingScheduleRowModel
        {
            Id = s.Id,
            FertilizerId = s.FertilizerId,
            FertilizerName = fertilizerMap.TryGetValue(s.FertilizerId, out var fertilizerName) ? fertilizerName : "—",
            Season = s.Season,
            FrequencyLabel = s.FrequencyDays.ToString(),
            Quantity = s.Quantity,
            Unit = s.Unit,
            Notes = s.Notes,
            DilutionInstructions = s.DilutionInstructions
        }).ToList();

        var logRows = recentLogs.Select(l => new FertilizingLogRowModel
        {
            Id = l.Id,
            AppliedAtUtc = l.AppliedAtUtc,
            FertilizerId = l.FertilizerId,
            FertilizerName = fertilizerMap.TryGetValue(l.FertilizerId, out var fertilizerName) ? fertilizerName : "—",
            Quantity = l.Quantity,
            Unit = l.Unit,
            Notes = l.Notes
        }).ToList();

        var model = new FertilizingScheduleTabModel
        {
            PlantInstanceId = id,
            Schedules = rows,
            LogRows = logRows,
            AvailableFertilizers = fertilizers
                .OrderBy(f => f.Name)
                .Select(f => new SelectListItem
                {
                    Value = f.Id.ToString(),
                    Text = f.Name
                })
                .ToList()
        };

        return PartialView("_FertilizingScheduleTab", model);
    }

    [HttpPost("{id:int}/fertilizing/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FertilizingScheduleCreate(int id, FertilizingScheduleModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return Json(new { success = false, message = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)) });

        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);

        try
        {
            var schedule = new FertilizingSchedule
            {
                OwnerId = ownerId,
                FertilizerId = model.FertilizerId,
                Season = model.Season,
                FrequencyDays = model.FrequencyDays,
                Quantity = model.Quantity,
                Unit = model.Unit,
                Notes = model.Notes,
                DilutionInstructions = model.DilutionInstructions
            };

            await _fertilizingService.CreateScheduleAsync(id, schedule, cancellationToken);
            return Json(new { success = true });
        }
        catch (DomainException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("{id:int}/fertilizing/{scheduleId:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FertilizingScheduleUpdate(int id, int scheduleId, FertilizingScheduleModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return Json(new { success = false, message = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)) });

        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);

        try
        {
            var schedule = new FertilizingSchedule
            {
                Id = scheduleId,
                OwnerId = ownerId,
                FertilizerId = model.FertilizerId,
                Season = model.Season,
                FrequencyDays = model.FrequencyDays,
                Quantity = model.Quantity,
                Unit = model.Unit,
                Notes = model.Notes,
                DilutionInstructions = model.DilutionInstructions
            };

            await _fertilizingService.UpdateScheduleAsync(id, schedule, cancellationToken);
            return Json(new { success = true });
        }
        catch (DomainException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("{id:int}/fertilizing/{scheduleId:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FertilizingScheduleDelete(int id, int scheduleId, CancellationToken cancellationToken)
    {
        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);

        try
        {
            await _fertilizingService.DeleteScheduleAsync(id, scheduleId, ownerId, cancellationToken);
            return Json(new { success = true });
        }
        catch (DomainException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("{id:int}/fertilizing/logs/{logId:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FertilizingLogDelete(int id, int logId, CancellationToken cancellationToken)
    {
        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);

        try
        {
            await _fertilizingService.DeleteLogAsync(id, logId, ownerId, cancellationToken);
            return Json(new { success = true });
        }
        catch (DomainException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    #endregion

    #region Container Repotting Actions

    [HttpGet("{id:int}/container")]
    public async Task<IActionResult> ContainerRepottingTab(int id, CancellationToken cancellationToken)
    {
        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);
        var instance = await _plantInstanceService.GetPlantInstanceByIdAsync(id, ownerId, includeDetails: false, cancellationToken);
        if (instance is null)
            return NotFound();

        var model = await BuildRepottingTabModelAsync(id, ownerId, cancellationToken);
        return PartialView("_ContainerRepottingTab", model);
    }

    [HttpPost("{id:int}/repotting/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RepottingCreate(int id, RepottingModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return Json(new { success = false, message = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)) });

        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);

        try
        {
            var repottingLog = new RepottingLog
            {
                OwnerId = ownerId,
                NewContainerId = model.NewContainerId,
                NewSoilMixId = model.NewSoilMixId,
                RepottedAtUtc = DateTime.SpecifyKind(model.RepottedOn.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc),
                Notes = model.Notes
            };

            await _plantInstanceService.RepotPlantAsync(id, repottingLog, cancellationToken);
            return Json(new { success = true });
        }
        catch (DomainException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("{id:int}/repotting/{logId:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RepottingUpdate(int id, int logId, RepottingModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return Json(new { success = false, message = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)) });

        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);

        try
        {
            var repottingLog = new RepottingLog
            {
                Id = logId,
                OwnerId = ownerId,
                NewContainerId = model.NewContainerId,
                NewSoilMixId = model.NewSoilMixId,
                RepottedAtUtc = DateTime.SpecifyKind(model.RepottedOn.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc),
                Notes = model.Notes
            };

            await _repottingLogService.UpdateLogAsync(id, repottingLog, cancellationToken);
            return Json(new { success = true });
        }
        catch (DomainException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("{id:int}/repotting/{logId:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RepottingDelete(int id, int logId, CancellationToken cancellationToken)
    {
        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);

        try
        {
            await _repottingLogService.DeleteLogAsync(id, logId, ownerId, cancellationToken);
            return Json(new { success = true });
        }
        catch (DomainException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("{id:int}/container/quick-create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ContainerQuickCreate(int id, QuickContainerCreateModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return Json(new { success = false, message = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)) });

        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);
        var instance = await _plantInstanceService.GetPlantInstanceByIdAsync(id, ownerId, includeDetails: false, cancellationToken);
        if (instance is null)
            return Json(new { success = false, message = "Plant instance not found." });

        try
        {
            var created = await _containerService.CreateContainerAsync(new Container
            {
                OwnerId = ownerId,
                ContainerType = model.ContainerType,
                Material = model.Material,
                BaseCircumferenceCm = model.BaseCircumferenceCm,
                RimCircumferenceCm = model.RimCircumferenceCm,
                HeightCm = model.HeightCm,
                LengthCm = model.LengthCm,
                WidthCm = model.WidthCm,
                VolumeL = model.VolumeL,
                Color = model.Color,
                HasDrainageHole = model.HasDrainageHole,
                Notes = model.Notes
            }, cancellationToken);

            return Json(new
            {
                success = true,
                data = new
                {
                    id = created.Id,
                    text = await BuildContainerDisplayNameAsync(created)
                }
            });
        }
        catch (DomainException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    private async Task<RepottingTabModel> BuildRepottingTabModelAsync(int plantInstanceId, Guid ownerId, CancellationToken cancellationToken)
    {
        var instance = await _plantInstanceService.GetPlantInstanceByIdAsync(plantInstanceId, ownerId, includeDetails: false, cancellationToken)
            ?? throw new DomainException("Plant instance not found.");

        var containersPaged = await _containerService.GetContainersAsync(ownerId, pageSize: int.MaxValue, cancellationToken: cancellationToken);
        var containers = containersPaged.ToList();
        var containerMap = containers.ToDictionary(c => c.Id);
        var containerDisplayMap = new Dictionary<int, string>(containers.Count);
        foreach (var containerItem in containers)
            containerDisplayMap[containerItem.Id] = await BuildContainerDisplayNameAsync(containerItem);

        var soilMixesPaged = await _soilMixService.GetSoilMixesAsync(pageSize: int.MaxValue, cancellationToken: cancellationToken);
        var soilMixes = soilMixesPaged.ToList();
        var soilMixMap = soilMixes.ToDictionary(s => s.Id);
        var currentSoilMixName = instance.SoilMixId.HasValue && soilMixMap.TryGetValue(instance.SoilMixId.Value, out var soilMix)
            ? soilMix.Name
            : "-";

        var logs = await _repottingLogService.GetLogsAsync(plantInstanceId, ownerId, pageIndex: 0, pageSize: 20, cancellationToken);

        var currentContainerEntity = instance.Container
            ?? containers.FirstOrDefault(containerItem => containerItem.PlantInstanceId == plantInstanceId);

        var currentContainerSinceUtc = currentContainerEntity is null
            ? (DateTime?)null
            : logs.Where(log => log.NewContainerId == currentContainerEntity.Id)
                .OrderByDescending(log => log.RepottedAtUtc)
                .Select(log => (DateTime?)log.RepottedAtUtc)
                .FirstOrDefault();

        var currentSoilMixIngredients = instance.SoilMixId.HasValue && soilMixMap.TryGetValue(instance.SoilMixId.Value, out var currentSoilMix)
            ? currentSoilMix.Ingredients
                .OrderByDescending(ingredientLine => ingredientLine.PercentageByVolume)
                .ThenBy(ingredientLine => ingredientLine.SoilIngredient.Name)
                .Select(ingredientLine => new SoilMixIngredientInfoModel
                {
                    IngredientName = ingredientLine.SoilIngredient.Name,
                    PercentageByVolume = ingredientLine.PercentageByVolume,
                    Notes = ingredientLine.Notes
                })
                .ToList()
            : new List<SoilMixIngredientInfoModel>();

        var currentContainer = currentContainerEntity is not null
            ? new ContainerInfoModel
            {
                ContainerId = currentContainerEntity.Id,
                ContainerName = containerDisplayMap[currentContainerEntity.Id],
                Material = currentContainerEntity.Material,
                Color = currentContainerEntity.Color,
                BaseCircumferenceCm = currentContainerEntity.BaseCircumferenceCm,
                RimCircumferenceCm = currentContainerEntity.RimCircumferenceCm,
                HeightCm = currentContainerEntity.HeightCm,
                LengthCm = currentContainerEntity.LengthCm,
                WidthCm = currentContainerEntity.WidthCm,
                VolumeL = currentContainerEntity.VolumeL,
                HasDrainageHole = currentContainerEntity.HasDrainageHole,
                Notes = currentContainerEntity.Notes,
                ContainerCreatedOnUtc = currentContainerEntity.CreatedAtUtc,
                ContainerUpdatedOnUtc = currentContainerEntity.UpdatedAtUtc,
                CurrentContainerSinceUtc = currentContainerSinceUtc,
                SoilMixId = instance.SoilMixId,
                SoilMixName = currentSoilMixName,
                SoilMixIngredients = currentSoilMixIngredients
            }
            : new ContainerInfoModel
            {
                ContainerName = "-",
                SoilMixId = instance.SoilMixId,
                SoilMixName = currentSoilMixName,
                SoilMixIngredients = currentSoilMixIngredients
            };

        return new RepottingTabModel
        {
            PlantInstanceId = plantInstanceId,
            CurrentContainer = currentContainer,
            Logs = logs.Select(log => new RepottingLogRowModel
            {
                Id = log.Id,
                PreviousContainerId = log.PreviousContainerId,
                PreviousContainerName = log.PreviousContainerId.HasValue && containerMap.TryGetValue(log.PreviousContainerId.Value, out var previousContainer)
                    ? containerDisplayMap[previousContainer.Id]
                    : "-",
                NewContainerId = log.NewContainerId,
                NewContainerName = log.NewContainerId.HasValue && containerMap.TryGetValue(log.NewContainerId.Value, out var newContainer)
                    ? containerDisplayMap[newContainer.Id]
                    : "-",
                PreviousSoilMixId = log.PreviousSoilMixId,
                PreviousSoilMixName = log.PreviousSoilMixId.HasValue && soilMixMap.TryGetValue(log.PreviousSoilMixId.Value, out var previousSoil)
                    ? previousSoil.Name
                    : "-",
                NewSoilMixId = log.NewSoilMixId,
                NewSoilMixName = log.NewSoilMixId.HasValue && soilMixMap.TryGetValue(log.NewSoilMixId.Value, out var newSoil)
                    ? newSoil.Name
                    : "-",
                RepottedOn = DateOnly.FromDateTime(log.RepottedAtUtc),
                ContainerChanged = log.ContainerChanged,
                SoilMixChanged = log.SoilMixChanged,
                Notes = log.Notes
            }).ToList(),
            AvailableContainers = new List<SelectListItem> { new() { Value = "", Text = "-" } }
                .Concat(containers.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = containerDisplayMap[c.Id]
                }))
                .ToList(),
            AvailableSoilMixes = new List<SelectListItem> { new() { Value = "", Text = "-" } }
                .Concat(soilMixes.Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                }))
                .ToList(),
            AvailableContainerTypes = Enum.GetValues<GardenContainerType>().Select(t => new SelectListItem
            {
                Value = t.ToString(),
                Text = t.ToString()
            }).ToList()
        };
    }

    private async Task<string> BuildContainerDisplayNameAsync(Container container)
    {
        var localizedContainerType = await _enumLocalizationHelper.GetLocalizedNameAsync(container.ContainerType);

        var prefix = !string.IsNullOrWhiteSpace(container.Material)
            ? container.Material
            : localizedContainerType;

        var parts = new List<string>();

        if (container.ContainerType == GardenContainerType.Bed)
        {
            var length = FormatRoundedLength(container.LengthCm);
            var width = FormatRoundedLength(container.WidthCm);

            if (length is not null && width is not null)
                parts.Add($"{length}x{width}");
        }
        else
        {
            if (container.VolumeL.HasValue && container.VolumeL.Value > 0)
                parts.Add($"{container.VolumeL.Value.ToString("0.##", CultureInfo.InvariantCulture)} λίτρα");

            var baseDiameter = FormatRoundedDiameter(container.BaseCircumferenceCm);
            if (baseDiameter is not null)
                parts.Add($"Β{baseDiameter}");

            var rimDiameter = FormatRoundedDiameter(container.RimCircumferenceCm);
            if (rimDiameter is not null)
                parts.Add($"Χ{rimDiameter}");

            var height = FormatRoundedLength(container.HeightCm);
            if (height is not null)
                parts.Add($"Υ{height}");
        }

        parts.Add(container.PlantInstanceId.HasValue ? "Κατειλημμένη" : "Κενή");

        return parts.Count == 0
            ? prefix
            : $"{prefix}, {string.Join(", ", parts)}";
    }

    private static string? FormatRoundedLength(decimal? value)
    {
        if (!value.HasValue || value.Value <= 0)
            return null;

        return Math.Round(value.Value, MidpointRounding.AwayFromZero).ToString(CultureInfo.InvariantCulture);
    }

    private static string? FormatRoundedDiameter(decimal? circumferenceCm)
    {
        if (!circumferenceCm.HasValue || circumferenceCm.Value <= 0)
            return null;

        var diameter = circumferenceCm.Value / (decimal)Math.PI;
        return Math.Round(diameter, MidpointRounding.AwayFromZero).ToString(CultureInfo.InvariantCulture);
    }

    #endregion
}