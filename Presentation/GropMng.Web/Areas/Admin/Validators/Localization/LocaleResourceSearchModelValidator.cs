using FluentValidation;
using GropMng.Web.Areas.Admin.Models.Localization;
using GropMng.Web.Areas.Admin.Validators.Common;

namespace GropMng.Web.Areas.Admin.Validators.Localization;

/// <summary>
/// Validator for <see cref="LocaleResourceSearchModel"/> used in the locale resources admin grid.
/// Validates DataTables paging and resource search filter parameters.
/// </summary>
public class LocaleResourceSearchModelValidator : BaseGropValidator<LocaleResourceSearchModel>
{
    public LocaleResourceSearchModelValidator()
    {
        RuleFor(x => x.LanguageId)
            .GreaterThan(0)
            .WithMessage("Language ID must be greater than zero.");

        RuleFor(x => x.Start)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Start offset cannot be negative.");

        RuleFor(x => x.Length)
            .InclusiveBetween(1, 200)
            .WithMessage("Page size must be between 1 and 200.");

        RuleFor(x => x.ResourceName)
            .MaximumLength(500)
            .WithMessage("Resource name filter cannot exceed 500 characters.");

        RuleFor(x => x.ResourceValue)
            .MaximumLength(4000)
            .WithMessage("Resource value filter cannot exceed 4000 characters.");
    }
}
