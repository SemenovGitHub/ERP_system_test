using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using FluentValidation;
using FluentValidation.Results;

namespace Domain.Validators.TimeEntries;

public sealed class DeleteTimeEntryValidator
    : AbstractValidator<TimeEntryModel>,
      IDeleteTimeEntryValidator
{
    private readonly ITimeEntryRepository _timeEntries;
    private readonly IPeriodRepository _periods;

    public DeleteTimeEntryValidator(
        ITimeEntryRepository timeEntries,
        IPeriodRepository periods)
    {
        _timeEntries = timeEntries;
        _periods = periods;

        RuleFor(x => x.Id).NotEmpty();

        When(x => x.Id != default, () => RuleFor(x => x).CustomAsync(ValidateBusinessAsync));
    }

    async Task IDomainValidator<TimeEntryModel>.ValidateAsync(
        TimeEntryModel instance,
        CancellationToken cancellationToken)
    {
        var result = await base.ValidateAsync(instance, cancellationToken);
        ThrowIfInvalid(result);
    }

    private async Task ValidateBusinessAsync(
        TimeEntryModel entry,
        ValidationContext<TimeEntryModel> context,
        CancellationToken cancellationToken)
    {
        var existing = await _timeEntries.GetByIdAsync(entry.Id, cancellationToken);
        if (existing is null)
        {
            context.AddFailure(new ValidationFailure(nameof(entry.Id), "Запись табеля не найдена.")
            {
                ErrorCode = ErrorCodes.NotFound
            });
            return;
        }

        var isClosed = await _periods.IsClosedAsync(
            existing.Date.Year,
            existing.Date.Month,
            cancellationToken);
        if (isClosed)
        {
            context.AddFailure(new ValidationFailure(
                nameof(entry.Id),
                $"Период {existing.Date.Month:00}.{existing.Date.Year} закрыт. Создавать, изменять и удалять записи нельзя.")
            {
                ErrorCode = ErrorCodes.ClosedPeriod
            });
        }
    }

    private static void ThrowIfInvalid(ValidationResult result)
    {
        if (result.IsValid)
        {
            return;
        }

        var business = result.Errors.FirstOrDefault(error =>
            error.ErrorCode is ErrorCodes.ClosedPeriod or ErrorCodes.NotFound);

        if (business is not null)
        {
            var status = business.ErrorCode == ErrorCodes.ClosedPeriod ? 409 : 404;
            throw new BusinessException(business.ErrorCode, business.ErrorMessage, status);
        }

        var errors = result.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.ErrorMessage).Distinct().ToArray());

        throw new Domain.Exceptions.ValidationException("Ошибка валидации запроса.", errors);
    }
}
