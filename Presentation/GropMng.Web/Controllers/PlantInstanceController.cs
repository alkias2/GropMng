using GropMng.Core.Common.Exceptions;
using GropMng.Core.Domain.Garden.Enums;
using GropMng.Core.Domain.Garden.Plants;
using GropMng.Core.Interfaces.Services.Garden.Locations;
using GropMng.Core.Interfaces.Services.Garden.Plants;
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

    #endregion

    #region Ctor

    public PlantInstanceController(
        IPlantInstanceService plantInstanceService,
        IPlantService plantService,
        ILocationService locationService,
        ICurrentOwnerProvider currentOwnerProvider)
    {
        _plantInstanceService = plantInstanceService ?? throw new ArgumentNullException(nameof(plantInstanceService));
        _plantService = plantService ?? throw new ArgumentNullException(nameof(plantService));
        _locationService = locationService ?? throw new ArgumentNullException(nameof(locationService));
        _currentOwnerProvider = currentOwnerProvider ?? throw new ArgumentNullException(nameof(currentOwnerProvider));
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
}
