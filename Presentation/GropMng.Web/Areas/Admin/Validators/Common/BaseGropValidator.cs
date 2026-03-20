using FluentValidation;

namespace GropMng.Web.Areas.Admin.Validators.Common;

/// <summary>
/// Base validator for Admin models. Centralizes shared validation helpers.
/// </summary>
public abstract class BaseGropValidator<TModel> : AbstractValidator<TModel>
{
    protected void RuleForDateRange(
        Func<TModel, DateTime?> fromSelector,
        Func<TModel, DateTime?> toSelector,
        string message)
    {
        RuleFor(model => model)
            .Must(model =>
            {
                var from = fromSelector(model);
                var to = toSelector(model);
                return !from.HasValue || !to.HasValue || from.Value <= to.Value;
            })
            .WithMessage(message);
    }
}
