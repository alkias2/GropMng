using FluentValidation;
using GropMng.Web.Areas.Admin.Models.Pesticide;
using GropMng.Web.Areas.Admin.Validators.Common;

namespace GropMng.Web.Areas.Admin.Validators.Pesticide;

/// <summary>
/// Validates create and edit requests for pesticide catalog entries.
/// </summary>
public class PesticideModelValidator : BaseGropValidator<PesticideModel>
{
    public PesticideModelValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required.")
            .MaximumLength(200)
            .WithMessage("Name cannot exceed 200 characters.");

        RuleFor(x => x.Brand)
            .MaximumLength(200)
            .WithMessage("Brand cannot exceed 200 characters.");

        RuleFor(x => x.ActiveIngredient)
            .MaximumLength(300)
            .WithMessage("Active Ingredient cannot exceed 300 characters.");
    }
}
