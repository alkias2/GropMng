using FluentValidation;
using GropMng.Web.Models.Garden;

namespace GropMng.Web.Validators.Garden;

public class ScheduleEditModelValidator : AbstractValidator<ScheduleEditModel>
{
    public ScheduleEditModelValidator()
    {
        RuleFor(x => x.ActionName)
            .NotEmpty().WithMessage("Action name is required.")
            .MaximumLength(300).WithMessage("Action name must not exceed 300 characters.");

        RuleFor(x => x.FrequencyValue)
            .NotEmpty()
            .GreaterThan(0).WithMessage("Frequency value must be greater than zero.");

        RuleFor(x => x.FrequencyUnit)
            .NotEmpty().WithMessage("Frequency unit is required.")
            .IsEnumName(typeof(Core.Domain.Garden.Enums.ScheduleFrequencyUnit), caseSensitive: false).WithMessage("Invalid frequency unit.");

        RuleFor(x => x.DosageNotes)
            .MaximumLength(500).WithMessage("Dosage notes must not exceed 500 characters.");

        RuleFor(x => x.StartDate)
            .NotEmpty();

        RuleFor(x => x.ScheduleStatus)
            .NotEmpty().WithMessage("Schedule status is required.")
            .IsEnumName(typeof(Core.Domain.Garden.Enums.ScheduleStatus), caseSensitive: false).WithMessage("Invalid schedule status.");
    }
}