using FluentValidation;
using GropMng.Web.Areas.Admin.Models;

namespace GropMng.Web.Areas.Admin.Validators;

public class DiseaseKnowledgeEditModelValidator : AbstractValidator<DiseaseKnowledgeEditModel>
{
    public DiseaseKnowledgeEditModelValidator()
    {
        RuleFor(x => x.CommonName)
            .NotEmpty().WithMessage("Common name is required.")
            .MaximumLength(300).WithMessage("Common name must not exceed 300 characters.");

        RuleFor(x => x.ScientificName)
            .MaximumLength(300).WithMessage("Scientific name must not exceed 300 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.");

        RuleFor(x => x.TreatmentGuidelines)
            .NotEmpty().WithMessage("Treatment guidelines are required.");
    }
}