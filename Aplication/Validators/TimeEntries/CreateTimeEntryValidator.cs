using Application.Interfaces;
using Application.Models.TimeEntries.Commands;
using Domain.Exceptions;
using FluentValidation;
using FluentValidation.Results;

namespace Application.Validators.TimeEntries;

public sealed class CreateTimeEntryValidator
    : AbstractValidator<CreateTimeEntryCommand>,
      IDomainValidator<CreateTimeEntryCommand>
{
    private readonly IEmployeeRepository _employees;
    private readonly IProjectRepository _projects;
    private readonly IPeriodRepository _periods;
    private readonly ITimeEntryRepository _timeEntries;

    public CreateTimeEntryValidator(
        IEmployeeRepository employees,
        IProjectRepository projects,
        IPeriodRepository periods,
        ITimeEntryRepository timeEntries)
    {
        _employees = employees;
        _projects = projects;
        _periods = periods;
        _timeEntries = timeEntries;

        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.Date).NotEqual(default(DateOnly));
        RuleFor(x => x.Hours)
            .Must(TimeEntryConstraints.IsValidEntryHours)
            .WithErrorCode(ErrorCodes.InvalidHours)
            .WithMessage("Часы должны быть положительными, кратными 0,5 и не больше 24 за одну запись.");
        RuleFor(x => x.Comment).MaximumLength(500);

        When(x =>
                x.EmployeeId != default
                && x.ProjectId != default
                && x.Date != default
                && TimeEntryConstraints.IsValidEntryHours(x.Hours),
            () => RuleFor(x => x).CustomAsync(ValidateBusinessAsync));
    }

    async Task IDomainValidator<CreateTimeEntryCommand>.ValidateAsync(
        CreateTimeEntryCommand instance,
        CancellationToken cancellationToken)
    {
        var result = await base.ValidateAsync(instance, cancellationToken);
        ThrowIfInvalid(result);
    }

    private async Task ValidateBusinessAsync(
        CreateTimeEntryCommand command,
        ValidationContext<CreateTimeEntryCommand> context,
        CancellationToken cancellationToken)
    {
        var isClosed = await _periods.IsClosedAsync(
            command.Date.Year,
            command.Date.Month,
            cancellationToken);
        if (isClosed)
        {
            context.AddFailure(Failure(
                nameof(command.Date),
                ErrorCodes.ClosedPeriod,
                $"Период {command.Date.Month:00}.{command.Date.Year} закрыт. Создавать, изменять и удалять записи нельзя."));
            return;
        }

        var employee = await _employees.GetByIdAsync(command.EmployeeId, cancellationToken);
        if (employee is null)
        {
            context.AddFailure(Failure(
                nameof(command.EmployeeId),
                ErrorCodes.NotFound,
                "Сотрудник не найден."));
            return;
        }

        var project = await _projects.GetByIdAsync(command.ProjectId, cancellationToken);
        if (project is null)
        {
            context.AddFailure(Failure(
                nameof(command.ProjectId),
                ErrorCodes.NotFound,
                "Проект не найден."));
            return;
        }

        if (command.Date < project.StartDate)
        {
            context.AddFailure(Failure(
                nameof(command.Date),
                ErrorCodes.ProjectDateOutOfRange,
                $"Дата записи {command.Date:dd.MM.yyyy} раньше начала проекта {project.Code} ({project.StartDate:dd.MM.yyyy})."));
            return;
        }

        if (project.EndDate is { } end && command.Date > end)
        {
            context.AddFailure(Failure(
                nameof(command.Date),
                ErrorCodes.ProjectDateOutOfRange,
                $"Дата записи {command.Date:dd.MM.yyyy} позже окончания проекта {project.Code} ({end:dd.MM.yyyy})."));
            return;
        }

        if (TimeEntryConstraints.FindRate(employee.Rates, command.Date) is null)
        {
            context.AddFailure(Failure(
                nameof(command.Date),
                ErrorCodes.NoRate,
                "На дату записи у сотрудника нет ни одной ставки. Запись создать нельзя."));
            return;
        }

        var hoursForDay = await _timeEntries.GetHoursForDayAsync(
            command.EmployeeId,
            command.Date,
            excludeEntryId: null,
            cancellationToken);
        var total = hoursForDay + command.Hours;
        if (total > TimeEntryConstraints.MaxHoursPerDay)
        {
            context.AddFailure(Failure(
                nameof(command.Hours),
                ErrorCodes.DailyHoursLimit,
                $"Суммарно у сотрудника за день не может быть больше {TimeEntryConstraints.MaxHoursPerDay} часов. " +
                $"Уже учтено {hoursForDay}, попытка добавить {command.Hours} (итого {total})."));
        }
    }

    private static void ThrowIfInvalid(ValidationResult result)
    {
        if (result.IsValid)
        {
            return;
        }

        var business = result.Errors.FirstOrDefault(error =>
            error.ErrorCode is
                ErrorCodes.NoRate or
                ErrorCodes.DailyHoursLimit or
                ErrorCodes.ClosedPeriod or
                ErrorCodes.ProjectDateOutOfRange or
                ErrorCodes.InvalidHours or
                ErrorCodes.NotFound);

        if (business is not null)
        {
            var status = business.ErrorCode switch
            {
                ErrorCodes.ClosedPeriod => 409,
                ErrorCodes.NotFound => 404,
                _ => 400
            };

            throw new BusinessException(business.ErrorCode, business.ErrorMessage, status);
        }

        var errors = result.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.ErrorMessage).Distinct().ToArray());

        throw new Domain.Exceptions.ValidationException("Ошибка валидации запроса.", errors);
    }

    private static ValidationFailure Failure(string property, string code, string message) =>
        new(property, message) { ErrorCode = code };
}
