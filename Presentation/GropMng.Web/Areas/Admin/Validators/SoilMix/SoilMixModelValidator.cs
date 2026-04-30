using FluentValidation;
using GropMng.Web.Areas.Admin.Models.SoilMix;
using GropMng.Web.Areas.Admin.Validators.Common;

namespace GropMng.Web.Areas.Admin.Validators.SoilMix;

/// <summary>
/// Validates create and edit requests for SoilMix catalog entries.
/// </summary>
public class SoilMixModelValidator : BaseGropValidator<SoilMixModel>
{
    public SoilMixModelValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required.")
            .MaximumLength(200)
            .WithMessage("Name cannot exceed 200 characters.");

        RuleFor(x => x.PhMin)
            .InclusiveBetween(0m, 14m)
            .When(x => x.PhMin.HasValue)
            .WithMessage("PhMin must be between 0 and 14.");

        RuleFor(x => x.PhMax)
            .InclusiveBetween(0m, 14m)
            .When(x => x.PhMax.HasValue)
            .WithMessage("PhMax must be between 0 and 14.");

        RuleFor(x => x)
            .Must(x => !x.PhMin.HasValue || !x.PhMax.HasValue || x.PhMin.Value <= x.PhMax.Value)
            .WithMessage("PhMin cannot be greater than PhMax.");
    }
}
