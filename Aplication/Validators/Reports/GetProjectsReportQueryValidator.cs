using Application.Models.Reports.Queries;
using FluentValidation;

namespace Application.Validators.Reports;

public sealed class GetProjectsReportQueryValidator : AbstractValidator<GetProjectsReportQuery>
{
    public GetProjectsReportQueryValidator()
    {
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100);
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
    }
}
