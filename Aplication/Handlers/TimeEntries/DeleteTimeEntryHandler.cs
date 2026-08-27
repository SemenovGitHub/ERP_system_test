using Application.Interfaces;
using Application.Models.TimeEntries.Commands;
using Application.Validators;
using Domain.Exceptions;
using MediatR;

namespace Application.Handlers.TimeEntries;

public sealed class DeleteTimeEntryHandler : IRequestHandler<DeleteTimeEntryCommand>
{
    private readonly ITimeEntryRepository _timeEntries;
    private readonly IDomainValidator<DeleteTimeEntryCommand> _validator;

    public DeleteTimeEntryHandler(
        ITimeEntryRepository timeEntries,
        IDomainValidator<DeleteTimeEntryCommand> validator)
    {
        _timeEntries = timeEntries;
        _validator = validator;
    }

    public async Task Handle(DeleteTimeEntryCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAsync(request, cancellationToken);

        var existing = await _timeEntries.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new BusinessException(ErrorCodes.NotFound, "Запись табеля не найдена.", 404);

        await _timeEntries.DeleteAsync(existing.Id, cancellationToken);
    }
}
