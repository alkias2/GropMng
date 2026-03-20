using FluentValidation;
using GropMng.Web.Areas.Admin.Models.Plant;
using GropMng.Web.Areas.Admin.Validators.Common;

namespace GropMng.Web.Areas.Admin.Validators.Plant;

/// <summary>
/// Validates create and edit requests for plant catalog entries.
/// </summary>
public class PlantModelValidator : BaseGropValidator<PlantModel>
{
    public PlantModelValidator()
    {
        RuleFor(x => x.CommonName)
            .NotEmpty()
            .WithMessage("Common Name is required.")
            .MaximumLength(200)
            .WithMessage("Common Name cannot exceed 200 characters.");

        RuleFor(x => x.ScientificName)
            .NotEmpty()
            .WithMessage("Scientific Name is required.")
            .MaximumLength(300)
            .WithMessage("Scientific Name cannot exceed 300 characters.");

        RuleFor(x => x.Family)
            .MaximumLength(200)
            .WithMessage("Family cannot exceed 200 characters.");

        RuleFor(x => x.MinTempCelsius)
            .InclusiveBetween(-50m, 80m)
            .When(x => x.MinTempCelsius.HasValue)
            .WithMessage("Minimum temperature must be between -50 and 80.");

        RuleFor(x => x.MaxTempCelsius)
            .InclusiveBetween(-50m, 80m)
            .When(x => x.MaxTempCelsius.HasValue)
            .WithMessage("Maximum temperature must be between -50 and 80.");

        RuleFor(x => x)
            .Must(x => !x.MinTempCelsius.HasValue || !x.MaxTempCelsius.HasValue || x.MinTempCelsius.Value <= x.MaxTempCelsius.Value)
            .WithMessage("Minimum temperature must be less than or equal to maximum temperature.");
    }
}
