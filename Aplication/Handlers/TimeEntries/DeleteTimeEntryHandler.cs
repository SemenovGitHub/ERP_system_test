using Application.Models.TimeEntries.Commands;
using AutoMapper;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using Domain.Validators;
using MediatR;

namespace Application.Handlers.TimeEntries;

public sealed class DeleteTimeEntryHandler : IRequestHandler<DeleteTimeEntryCommand>
{
    private readonly ITimeEntryRepository _timeEntries;
    private readonly IDeleteTimeEntryValidator _validator;
    private readonly IMapper _mapper;

    public DeleteTimeEntryHandler(
        ITimeEntryRepository timeEntries,
        IDeleteTimeEntryValidator validator,
        IMapper mapper)
    {
        _timeEntries = timeEntries;
        _validator = validator;
        _mapper = mapper;
    }

    public async Task Handle(DeleteTimeEntryCommand request, CancellationToken cancellationToken)
    {
        var model = _mapper.Map<TimeEntryModel>(request);
        await _validator.ValidateAsync(model, cancellationToken);

        var existing = await _timeEntries.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new BusinessException(ErrorCodes.NotFound, "Запись табеля не найдена.", 404);

        await _timeEntries.DeleteAsync(existing.Id, cancellationToken);
    }
}
