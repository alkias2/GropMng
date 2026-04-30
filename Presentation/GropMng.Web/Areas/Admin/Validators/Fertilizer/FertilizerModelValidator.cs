using FluentValidation;
using GropMng.Web.Areas.Admin.Models.Fertilizer;
using GropMng.Web.Areas.Admin.Validators.Common;

namespace GropMng.Web.Areas.Admin.Validators.Fertilizer;

/// <summary>
/// Validates create and edit requests for fertilizer catalog entries.
/// </summary>
public class FertilizerModelValidator : BaseGropValidator<FertilizerModel>
{
    public FertilizerModelValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required.")
            .MaximumLength(200)
            .WithMessage("Name cannot exceed 200 characters.");

        RuleFor(x => x.Brand)
            .MaximumLength(200)
            .WithMessage("Brand cannot exceed 200 characters.");

        RuleFor(x => x.NpkRatio)
            .MaximumLength(50)
            .WithMessage("NPK Ratio cannot exceed 50 characters.");
    }
}
