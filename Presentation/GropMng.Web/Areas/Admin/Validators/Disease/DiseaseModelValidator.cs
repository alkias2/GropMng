using FluentValidation;
using GropMng.Web.Areas.Admin.Models.Disease;
using GropMng.Web.Areas.Admin.Validators.Common;

namespace GropMng.Web.Areas.Admin.Validators.Disease;

/// <summary>
/// Validates create and edit requests for disease catalog entries.
/// </summary>
public class DiseaseModelValidator : BaseGropValidator<DiseaseModel>
{
    public DiseaseModelValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required.")
            .MaximumLength(200)
            .WithMessage("Name cannot exceed 200 characters.");

        RuleFor(x => x.AffectedParts)
            .MaximumLength(300)
            .WithMessage("Affected Parts cannot exceed 300 characters.");
    }
}
