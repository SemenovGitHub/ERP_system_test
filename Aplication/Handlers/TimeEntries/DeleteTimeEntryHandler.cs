using Application.Models.TimeEntries.Commands;
using AutoMapper;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using Domain.Validators;
using Domain.Validators.TimeEntries;
using MediatR;

namespace Application.Handlers.TimeEntries;

public sealed class DeleteTimeEntryHandler : IRequestHandler<DeleteTimeEntryCommand>
{
    private readonly ITimeEntryRepository _timeEntriesRepository;
    private readonly ITimeEntryValidator _validator;
    private readonly IMapper _mapper;

    public DeleteTimeEntryHandler(
        ITimeEntryRepository timeEntriesRepository,
        IEnumerable<ITimeEntryValidator> validators,
        IMapper mapper)
    {
        _timeEntriesRepository = timeEntriesRepository;
        _validator = validators
            .Single(validator => validator.Name == DeleteTimeEntryValidator.Name);
        _mapper = mapper;
    }

    public async Task Handle(DeleteTimeEntryCommand request, CancellationToken cancellationToken)
    {
        var model = _mapper.Map<TimeEntryModel>(request);
        await _validator.ValidateAsync(model, cancellationToken).ThrowIfInvalidAsync();

        var existing = await _timeEntriesRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new BusinessException(ErrorCodes.NotFound, "Запись табеля не найдена.", 404);

        await _timeEntriesRepository.DeleteAsync(existing.Id, cancellationToken);
    }
}
