using GropMng.Core.Common.Exceptions;
using GropMng.Core.Domain.Garden.Enums;
using GropMng.Core.Domain.Garden.Locations;
using GropMng.Core.Interfaces.Services.Garden.Locations;
using GropMng.Core.Interfaces.Services.Media;
using GropMng.Core.Interfaces.Services.User;
using GropMng.Web.Models.Garden;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GropMng.Web.Controllers;

/// <summary>
/// Manages owner GardenSpots nested under a Location.
/// </summary>
[Authorize]
[Route("my-garden/locations/{locationId:int}/spots")]
public class GardenSpotController : Controller
{
    #region Fields

    private readonly ILocationService _locationService;
    private readonly ICurrentOwnerProvider _currentOwnerProvider;
    private readonly IPictureService _pictureService;

    #endregion

    #region Ctor

    public GardenSpotController(
        ILocationService locationService,
        ICurrentOwnerProvider currentOwnerProvider,
        IPictureService pictureService)
    {
        _locationService = locationService ?? throw new ArgumentNullException(nameof(locationService));
        _currentOwnerProvider = currentOwnerProvider ?? throw new ArgumentNullException(nameof(currentOwnerProvider));
        _pictureService = pictureService ?? throw new ArgumentNullException(nameof(pictureService));
    }

    #endregion

    #region Public

    [HttpGet("create")]
    public async Task<IActionResult> Create(int locationId, CancellationToken cancellationToken)
    {
        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);
        var location = await _locationService.GetLocationByIdAsync(locationId, ownerId, cancellationToken: cancellationToken);

        if (location is null)
            return RedirectToAction("List", "Location");

        return View(PrepareModel(new GardenSpotModel
        {
            LocationId = locationId,
            LocationName = location.Name
        }));
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int locationId, GardenSpotModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(PrepareModel(model));

        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);

        try
        {
            await _locationService.AddGardenSpotAsync(locationId, new GardenSpot
            {
                OwnerId = ownerId,
                LocationId = locationId,
                Name = model.Name,
                Orientation = model.Orientation,
                CoverType = model.CoverType,
                SunHoursPerDay = model.SunHoursPerDay,
                Surroundings = model.Surroundings,
                Notes = model.Notes,
                PictureId = model.PictureId
            }, cancellationToken);

            TempData["SuccessMessage"] = "Garden spot created successfully.";
            return RedirectToAction("Edit", "Location", new { id = locationId });
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(PrepareModel(model));
        }
    }

    [HttpGet("{spotId:int}/edit")]
    public async Task<IActionResult> Edit(int locationId, int spotId, CancellationToken cancellationToken)
    {
        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);
        var spots = await _locationService.GetGardenSpotsAsync(locationId, ownerId, cancellationToken);
        var spot = spots.FirstOrDefault(s => s.Id == spotId);

        if (spot is null)
            return RedirectToAction("Edit", "Location", new { id = locationId });

        var location = await _locationService.GetLocationByIdAsync(locationId, ownerId, cancellationToken: cancellationToken);

        return View(PrepareModel(new GardenSpotModel
        {
            Id = spot.Id,
            LocationId = locationId,
            LocationName = location?.Name ?? string.Empty,
            Name = spot.Name,
            Orientation = spot.Orientation,
            CoverType = spot.CoverType,
            SunHoursPerDay = spot.SunHoursPerDay,
            Surroundings = spot.Surroundings,
            Notes = spot.Notes,
            PictureId = spot.PictureId
        }));
    }

    [HttpPost("{spotId:int}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int locationId, int spotId, GardenSpotModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(PrepareModel(model));

        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);

        try
        {
            var spots = await _locationService.GetGardenSpotsAsync(locationId, ownerId, cancellationToken);
            var existing = spots.FirstOrDefault(s => s.Id == spotId);
            var previousPictureId = existing?.PictureId ?? 0;

            await _locationService.UpdateGardenSpotAsync(locationId, new GardenSpot
            {
                Id = spotId,
                OwnerId = ownerId,
                LocationId = locationId,
                Name = model.Name,
                Orientation = model.Orientation,
                CoverType = model.CoverType,
                SunHoursPerDay = model.SunHoursPerDay,
                Surroundings = model.Surroundings,
                Notes = model.Notes,
                PictureId = model.PictureId
            }, cancellationToken);

            // Delete old picture when replaced or removed
            if (previousPictureId > 0 && previousPictureId != model.PictureId)
            {
                var oldPicture = await _pictureService.GetPictureByIdAsync(previousPictureId);
                if (oldPicture != null)
                    await _pictureService.DeletePictureAsync(oldPicture);
            }

            TempData["SuccessMessage"] = "Garden spot updated successfully.";
            return RedirectToAction(nameof(Edit), new { locationId, spotId });
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(PrepareModel(model));
        }
    }

    [HttpPost("{spotId:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int locationId, int spotId, CancellationToken cancellationToken)
    {
        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);

        try
        {
            await _locationService.DeleteGardenSpotAsync(locationId, spotId, ownerId, cancellationToken);
            TempData["SuccessMessage"] = "Garden spot deleted successfully.";
        }
        catch (DomainException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction("Edit", "Location", new { id = locationId });
    }

    #endregion

    #region Private

    private static GardenSpotModel PrepareModel(GardenSpotModel model)
    {
        model.AvailableOrientations = new List<SelectListItem>
        {
            new() { Value = "", Text = "— Select orientation —" }
        }.Concat(Enum.GetValues<GardenOrientation>().Select(o => new SelectListItem
        {
            Value = o.ToString(),
            Text = o.ToString(),
            Selected = model.Orientation == o
        })).ToList();

        model.AvailableCoverTypes = new List<SelectListItem>
        {
            new() { Value = "", Text = "— Select cover type —" }
        }.Concat(Enum.GetValues<GardenCoverType>().Select(c => new SelectListItem
        {
            Value = c.ToString(),
            Text = c.ToString(),
            Selected = model.CoverType == c
        })).ToList();

        return model;
    }

    #endregion
}
