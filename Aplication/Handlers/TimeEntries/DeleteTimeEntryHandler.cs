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
    private readonly ITimeEntryRepository _timeEntriesRepository;
    private readonly IDeleteTimeEntryValidator _validator;
    private readonly IMapper _mapper;

    public DeleteTimeEntryHandler(
        ITimeEntryRepository timeEntriesRepository,
        IDeleteTimeEntryValidator validator,
        IMapper mapper)
    {
        _timeEntriesRepository = timeEntriesRepository;
        _validator = validator;
        _mapper = mapper;
    }

    public async Task Handle(DeleteTimeEntryCommand request, CancellationToken cancellationToken)
    {
        var model = _mapper.Map<TimeEntryModel>(request);
        await _validator.ValidateAsync(model, cancellationToken);

        var existing = await _timeEntriesRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new BusinessException(ErrorCodes.NotFound, "Запись табеля не найдена.", 404);

        await _timeEntriesRepository.DeleteAsync(existing.Id, cancellationToken);
    }
}
