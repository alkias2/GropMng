using FluentValidation;
using GropMng.Web.Factories.Settings;
using GropMng.Web.Areas.Admin.Models.Settings;
using Microsoft.AspNetCore.Mvc;

namespace GropMng.Web.Areas.Admin.Controllers;

[Area("Admin")]
public class SettingController : Controller
{
    private readonly ISettingModelFactory _settingModelFactory;
    private readonly IValidator<GropAdminAreaSettingsModel> _settingsValidator;

    public SettingController(
        ISettingModelFactory settingModelFactory,
        IValidator<GropAdminAreaSettingsModel> settingsValidator)
    {
        _settingModelFactory = settingModelFactory;
        _settingsValidator = settingsValidator;
    }

    [HttpGet]
    public async Task<IActionResult> AdminArea()
    {
        var model = await _settingModelFactory.PrepareAdminAreaSettingsModelAsync();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdminArea(GropAdminAreaSettingsModel model)
    {
        var validationResult = await _settingsValidator.ValidateAsync(model);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
        }

        if (!ModelState.IsValid)
            return View(model);

        await _settingModelFactory.SaveAdminAreaSettingsAsync(model);

        TempData["SuccessMessage"] = "Admin settings were updated successfully.";
        return RedirectToAction(nameof(AdminArea));
    }
}
