using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using FluentValidation;
using FluentValidation.Results;

namespace Domain.Validators.TimeEntries;

public sealed class DeleteTimeEntryValidator : AbstractValidator<TimeEntryModel>, ITimeEntryValidator
{
    public const string Name = nameof(DeleteTimeEntryValidator);

    string ITimeEntryValidator.Name => Name;
    private readonly ITimeEntryRepository _timeEntriesRepository;
    private readonly IPeriodRepository _periodsRepository;

    public DeleteTimeEntryValidator(
        ITimeEntryRepository timeEntriesRepository,
        IPeriodRepository periodsRepository)
    {
        _timeEntriesRepository = timeEntriesRepository;
        _periodsRepository = periodsRepository;

        RuleFor(x => x.Id).NotEmpty();

        When(x => x.Id != default, () => RuleFor(x => x).CustomAsync(ValidateBusinessAsync));
    }

    private async Task ValidateBusinessAsync(
        TimeEntryModel entry,
        ValidationContext<TimeEntryModel> context,
        CancellationToken cancellationToken)
    {
        var existing = await _timeEntriesRepository.GetByIdAsync(entry.Id, cancellationToken);
        if (existing is null)
        {
            context.AddFailure(new ValidationFailure(nameof(entry.Id), "Запись табеля не найдена.")
            {
                ErrorCode = ErrorCodes.NotFound
            });
            return;
        }

        var isClosed = await _periodsRepository.IsClosedAsync(
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
}
