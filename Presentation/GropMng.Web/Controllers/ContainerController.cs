using GropMng.Core.Common.Exceptions;
using GropMng.Core.Domain.Garden.Enums;
using GropMng.Core.Domain.Garden.Plants;
using GropMng.Core.Interfaces.Services.Garden.Plants;
using GropMng.Core.Interfaces.Services.User;
using GropMng.Web.Models.Garden;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GropMng.Web.Controllers;

/// <summary>
/// Manages owner Containers (My Garden → Containers).
/// </summary>
[Authorize]
[Route("my-garden/containers")]
public class ContainerController : Controller
{
    #region Fields

    private readonly IContainerService _containerService;
    private readonly ICurrentOwnerProvider _currentOwnerProvider;

    #endregion

    #region Ctor

    public ContainerController(IContainerService containerService, ICurrentOwnerProvider currentOwnerProvider)
    {
        _containerService = containerService ?? throw new ArgumentNullException(nameof(containerService));
        _currentOwnerProvider = currentOwnerProvider ?? throw new ArgumentNullException(nameof(currentOwnerProvider));
    }

    #endregion

    #region Public

    [HttpGet("")]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);
        var containers = await _containerService.GetContainersAsync(ownerId, cancellationToken: cancellationToken);
        return View(containers.ToList());
    }

    [HttpGet("create")]
    public IActionResult Create()
    {
        return View(PrepareModel(new ContainerModel()));
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ContainerModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(PrepareModel(model));

        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);

        try
        {
            await _containerService.CreateContainerAsync(new Container
            {
                OwnerId = ownerId,
                ContainerType = model.ContainerType,
                Material = model.Material,
                LengthCm = model.LengthCm,
                WidthCm = model.WidthCm,
                DepthCm = model.DepthCm,
                DiameterCm = model.DiameterCm,
                VolumeL = model.VolumeL,
                Color = model.Color,
                HasDrainageHole = model.HasDrainageHole,
                Notes = model.Notes
            }, cancellationToken);

            TempData["SuccessMessage"] = "Container added successfully.";
            return RedirectToAction(nameof(List));
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(PrepareModel(model));
        }
    }

    [HttpGet("{id:int}/edit")]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);
        var container = await _containerService.GetContainerByIdAsync(id, ownerId, cancellationToken);

        if (container is null)
            return RedirectToAction(nameof(List));

        return View(PrepareModel(new ContainerModel
        {
            Id = container.Id,
            ContainerType = container.ContainerType,
            Material = container.Material,
            LengthCm = container.LengthCm,
            WidthCm = container.WidthCm,
            DepthCm = container.DepthCm,
            DiameterCm = container.DiameterCm,
            VolumeL = container.VolumeL,
            Color = container.Color,
            HasDrainageHole = container.HasDrainageHole,
            Notes = container.Notes,
            PlantInstanceCount = container.PlantInstances.Count
        }));
    }

    [HttpPost("{id:int}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ContainerModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(PrepareModel(model));

        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);
        var container = await _containerService.GetContainerByIdAsync(id, ownerId, cancellationToken);

        if (container is null)
            return RedirectToAction(nameof(List));

        try
        {
            container.ContainerType = model.ContainerType;
            container.Material = model.Material;
            container.LengthCm = model.LengthCm;
            container.WidthCm = model.WidthCm;
            container.DepthCm = model.DepthCm;
            container.DiameterCm = model.DiameterCm;
            container.VolumeL = model.VolumeL;
            container.Color = model.Color;
            container.HasDrainageHole = model.HasDrainageHole;
            container.Notes = model.Notes;

            await _containerService.UpdateContainerAsync(container, cancellationToken);

            TempData["SuccessMessage"] = "Container updated successfully.";
            return RedirectToAction(nameof(Edit), new { id });
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(PrepareModel(model));
        }
    }

    [HttpPost("{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);

        try
        {
            await _containerService.DeleteContainerAsync(id, ownerId, cancellationToken);
            TempData["SuccessMessage"] = "Container deleted successfully.";
        }
        catch (DomainException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(List));
    }

    #endregion

    #region Private

    private static ContainerModel PrepareModel(ContainerModel model)
    {
        model.AvailableContainerTypes = Enum.GetValues<GardenContainerType>().Select(t => new SelectListItem
        {
            Value = t.ToString(),
            Text = t.ToString(),
            Selected = model.ContainerType == t
        }).ToList();

        return model;
    }

    #endregion
}
