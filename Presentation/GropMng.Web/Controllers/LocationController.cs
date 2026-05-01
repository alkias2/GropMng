using GropMng.Core.Common.Exceptions;
using GropMng.Core.Domain.Garden.Locations;
using GropMng.Core.Interfaces.Services.Garden.Locations;
using GropMng.Core.Interfaces.Services.User;
using GropMng.Web.Models.Garden;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GropMng.Web.Controllers;

/// <summary>
/// Manages owner Locations (My Garden → Locations).
/// </summary>
[Authorize]
[Route("my-garden/locations")]
public class LocationController : Controller
{
    #region Fields

    private readonly ILocationService _locationService;
    private readonly ICurrentOwnerProvider _currentOwnerProvider;

    #endregion

    #region Ctor

    public LocationController(ILocationService locationService, ICurrentOwnerProvider currentOwnerProvider)
    {
        _locationService = locationService ?? throw new ArgumentNullException(nameof(locationService));
        _currentOwnerProvider = currentOwnerProvider ?? throw new ArgumentNullException(nameof(currentOwnerProvider));
    }

    #endregion

    #region Public

    [HttpGet("")]
    [HttpGet("/Location/List")]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);
        var locations = await _locationService.GetLocationsAsync(ownerId, cancellationToken: cancellationToken);
        return View(locations.ToList());
    }

    [HttpGet("create")]
    public IActionResult Create()
    {
        return View(new LocationModel());
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LocationModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);

        try
        {
            await _locationService.CreateLocationAsync(new Location
            {
                OwnerId = ownerId,
                Name = model.Name,
                City = model.City,
                Country = model.Country,
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                ClimateZone = model.ClimateZone,
                Notes = model.Notes
            }, cancellationToken);

            TempData["SuccessMessage"] = "Location created successfully.";
            return RedirectToAction(nameof(List));
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpGet("{id:int}/edit")]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);
        var location = await _locationService.GetLocationByIdAsync(id, ownerId, includeGardenSpots: true, cancellationToken);

        if (location is null)
            return RedirectToAction(nameof(List));

        return View(MapToModel(location));
    }

    [HttpPost("{id:int}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, LocationModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);
        var location = await _locationService.GetLocationByIdAsync(id, ownerId, includeGardenSpots: true, cancellationToken);

        if (location is null)
            return RedirectToAction(nameof(List));

        try
        {
            location.Name = model.Name;
            location.City = model.City;
            location.Country = model.Country;
            location.Latitude = model.Latitude;
            location.Longitude = model.Longitude;
            location.ClimateZone = model.ClimateZone;
            location.Notes = model.Notes;

            await _locationService.UpdateLocationAsync(location, cancellationToken);

            TempData["SuccessMessage"] = "Location updated successfully.";
            return RedirectToAction(nameof(Edit), new { id });
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            model.GardenSpots = location.GardenSpots.Select(s => new GardenSpotRowModel
            {
                Id = s.Id,
                Name = s.Name,
                OrientationDisplay = s.Orientation?.ToString(),
                CoverTypeDisplay = s.CoverType?.ToString(),
                SunHoursPerDay = s.SunHoursPerDay
            }).ToList();
            return View(model);
        }
    }

    [HttpPost("{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);

        try
        {
            await _locationService.DeleteLocationAsync(id, ownerId, cancellationToken);
            TempData["SuccessMessage"] = "Location deleted successfully.";
        }
        catch (DomainException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(List));
    }

    #endregion

    #region Private

    private static LocationModel MapToModel(Location location) => new()
    {
        Id = location.Id,
        Name = location.Name,
        City = location.City,
        Country = location.Country,
        Latitude = location.Latitude,
        Longitude = location.Longitude,
        ClimateZone = location.ClimateZone,
        Notes = location.Notes,
        GardenSpots = location.GardenSpots.Select(s => new GardenSpotRowModel
        {
            Id = s.Id,
            Name = s.Name,
            OrientationDisplay = s.Orientation?.ToString(),
            CoverTypeDisplay = s.CoverType?.ToString(),
            SunHoursPerDay = s.SunHoursPerDay
        }).ToList()
    };

    #endregion
}
