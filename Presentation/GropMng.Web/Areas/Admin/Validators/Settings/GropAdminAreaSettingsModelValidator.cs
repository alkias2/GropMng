using FluentValidation;
using GropMng.Web.Areas.Admin.Models.Settings;
using GropMng.Web.Areas.Admin.Validators.Common;

namespace GropMng.Web.Areas.Admin.Validators.Settings;

public class GropAdminAreaSettingsModelValidator : BaseGropValidator<GropAdminAreaSettingsModel>
{
    public GropAdminAreaSettingsModelValidator()
    {
        RuleFor(x => x.DefaultGridPageSize)
            .InclusiveBetween(1, 200)
            .WithMessage("Default grid page size must be between 1 and 200.");

        RuleFor(x => x.GridPageSizes)
            .NotEmpty()
            .WithMessage("Grid page sizes are required.")
            .MaximumLength(100)
            .WithMessage("Grid page sizes value is too long.");

        RuleFor(x => x.RichEditorProvider)
            .NotEmpty()
            .WithMessage("Rich editor provider is required.")
            .MaximumLength(40)
            .WithMessage("Rich editor provider cannot exceed 40 characters.");

        RuleFor(x => x.AdminDashboardWelcomeHtml)
            .MaximumLength(8000)
            .WithMessage("Welcome HTML cannot exceed 8000 characters.");
    }
}
