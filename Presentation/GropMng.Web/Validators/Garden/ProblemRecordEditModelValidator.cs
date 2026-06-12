using FluentValidation;
using GropMng.Web.Models.Garden;

namespace GropMng.Web.Validators.Garden;

public class ProblemRecordEditModelValidator : AbstractValidator<ProblemRecordEditModel>
{
    public ProblemRecordEditModelValidator()
    {
        RuleFor(x => x.ProblemName)
            .NotEmpty().WithMessage("Problem name is required.")
            .MaximumLength(300).WithMessage("Problem name must not exceed 300 characters.");

        RuleFor(x => x.DetectedDate)
            .NotEmpty()
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow)).WithMessage("Detected date cannot be in the future.");

        RuleFor(x => x.Severity)
            .NotEmpty().WithMessage("Severity is required.")
            .IsEnumName(typeof(Core.Domain.Garden.Enums.Severity), caseSensitive: false).WithMessage("Invalid severity value.");

        RuleFor(x => x.ProblemStatus)
            .NotEmpty().WithMessage("Problem status is required.")
            .IsEnumName(typeof(Core.Domain.Garden.Enums.ProblemStatus), caseSensitive: false).WithMessage("Invalid problem status value.");

        RuleFor(x => x.InfoSource)
            .NotEmpty().WithMessage("Information source is required.")
            .IsEnumName(typeof(Core.Domain.Garden.Enums.InfoSource), caseSensitive: false).WithMessage("Invalid information source value.");

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes must not exceed 2000 characters.");
    }
}