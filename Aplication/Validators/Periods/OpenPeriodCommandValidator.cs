using Application.Models.Periods.Commands;
using FluentValidation;

namespace Application.Validators.Periods;

public sealed class OpenPeriodCommandValidator : AbstractValidator<OpenPeriodCommand>
{
    public OpenPeriodCommandValidator()
    {
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100);
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
    }
}
