using Domain.Models;
using FluentValidation;

namespace Domain.Validators.Periods;

public sealed class PeriodValidator : AbstractValidator<PeriodModel>
{
    public PeriodValidator()
    {
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100);
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
    }
}
