using Application.Interfaces;
using Application.Models.TimeEntries.Commands;
using Domain.Exceptions;
using Domain.Rules;
using MediatR;

namespace Application.Handlers.TimeEntries;

public sealed class DeleteTimeEntryHandler : IRequestHandler<DeleteTimeEntryCommand>
{
    private readonly ITimeEntryRepository _timeEntries;
    private readonly IPeriodRepository _periods;

    public DeleteTimeEntryHandler(
        ITimeEntryRepository timeEntries,
        IPeriodRepository periods)
    {
        _timeEntries = timeEntries;
        _periods = periods;
    }

    public async Task Handle(DeleteTimeEntryCommand request, CancellationToken cancellationToken)
    {
        var existing = await _timeEntries.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new BusinessException(ErrorCodes.NotFound, "Запись табеля не найдена.", 404);

        var isClosed = await _periods.IsClosedAsync(
            existing.Date.Year,
            existing.Date.Month,
            cancellationToken);
        ClosedPeriodRules.EnsureOpen(isClosed, existing.Date);

        await _timeEntries.DeleteAsync(existing.Id, cancellationToken);
    }
}
