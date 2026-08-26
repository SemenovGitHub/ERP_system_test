using Application.Models.TimeEntries.Queries;
using FluentValidation;

namespace Application.Validators.TimeEntries;

public sealed class GetTimeEntriesQueryValidator : AbstractValidator<GetTimeEntriesQuery>
{
    public GetTimeEntriesQueryValidator()
    {
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100);
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
