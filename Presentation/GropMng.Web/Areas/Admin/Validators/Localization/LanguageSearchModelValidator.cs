using FluentValidation;
using GropMng.Web.Areas.Admin.Models.Localization;
using GropMng.Web.Areas.Admin.Validators.Common;

namespace GropMng.Web.Areas.Admin.Validators.Localization;

/// <summary>
/// Validator for <see cref="LanguageSearchModel"/> used in the Languages admin grid.
/// Validates DataTables paging and search filter parameters.
/// </summary>
public class LanguageSearchModelValidator : BaseGropValidator<LanguageSearchModel>
{
    public LanguageSearchModelValidator()
    {
        RuleFor(x => x.Start)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Start offset cannot be negative.");

        RuleFor(x => x.Length)
            .InclusiveBetween(1, 200)
            .WithMessage("Page size must be between 1 and 200.");

        RuleFor(x => x.Name)
            .MaximumLength(500)
            .WithMessage("Name filter cannot exceed 500 characters.");
    }
}
