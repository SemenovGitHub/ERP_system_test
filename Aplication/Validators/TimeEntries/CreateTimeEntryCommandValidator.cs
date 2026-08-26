using Application.Models.TimeEntries.Commands;
using Domain.Rules;
using FluentValidation;

namespace Application.Validators.TimeEntries;

public sealed class CreateTimeEntryCommandValidator : AbstractValidator<CreateTimeEntryCommand>
{
    public CreateTimeEntryCommandValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.Date).NotEqual(default(DateOnly));
        RuleFor(x => x.Hours)
            .Must(HoursRules.IsValidEntryHours)
            .WithMessage("Часы должны быть положительными, кратными 0,5 и не больше 24.");
        RuleFor(x => x.Comment).MaximumLength(500);
    }
}
