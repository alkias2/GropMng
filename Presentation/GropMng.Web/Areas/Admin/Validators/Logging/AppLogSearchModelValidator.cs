using FluentValidation;
using GropMng.Web.Areas.Admin.Models.Logging;
using GropMng.Web.Areas.Admin.Validators.Common;

namespace GropMng.Web.Areas.Admin.Validators.Logging;

public class AppLogSearchModelValidator : BaseGropValidator<AppLogSearchModel>
{
    public AppLogSearchModelValidator()
    {
        RuleFor(x => x.Start)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Start offset cannot be negative.");

        RuleFor(x => x.Length)
            .InclusiveBetween(1, 200)
            .WithMessage("Page size must be between 1 and 200.");

        RuleFor(x => x.Level)
            .MaximumLength(40)
            .WithMessage("Level filter cannot exceed 40 characters.");

        RuleForDateRange(x => x.FromDate, x => x.ToDate, "From Date must be earlier than or equal to To Date.");
    }
}
