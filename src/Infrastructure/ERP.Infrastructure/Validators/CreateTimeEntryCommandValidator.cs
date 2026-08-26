using ERP.Domain.Rules;
using ERP.Infrastructure.Commands;
using FluentValidation;

namespace ERP.Infrastructure.Validators;

/// <summary>
/// Validator for CreateTimeEntryCommand that validates input data format
/// Business logic validation is performed in the handler
/// </summary>
public sealed class CreateTimeEntryCommandValidator : AbstractValidator<CreateTimeEntryCommand>
{
    public CreateTimeEntryCommandValidator()
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty()
            .WithMessage("ID сотрудника обязателен");

        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage("ID проекта обязателен");

        RuleFor(x => x.Date)
            .NotEqual(default(DateOnly))
            .WithMessage("Дата обязательна");

        RuleFor(x => x.Hours)
            .Must(HoursRules.IsValidEntryHours)
            .WithMessage("Часы должны быть положительными, кратными 0,5 и не больше 24");

        RuleFor(x => x.Comment)
            .MaximumLength(500)
            .WithMessage("Комментарий не должен превышать 500 символов");
    }
}