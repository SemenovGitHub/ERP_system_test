using Application.Models.Periods.Commands;
using AutoMapper;
using Domain.Interfaces;
using Domain.Models;
using Domain.Validators;
using FluentValidation;
using MediatR;

namespace Application.Handlers.Periods;

public sealed class OpenPeriodHandler : IRequestHandler<OpenPeriodCommand>
{
    private readonly IPeriodRepository _periodsRepository;
    private readonly IValidator<PeriodModel> _validator;
    private readonly IMapper _mapper;

    public OpenPeriodHandler(
        IPeriodRepository periodsRepository,
        IValidator<PeriodModel> validator,
        IMapper mapper)
    {
        _periodsRepository = periodsRepository;
        _validator = validator;
        _mapper = mapper;
    }

    public async Task Handle(OpenPeriodCommand request, CancellationToken cancellationToken)
    {
        var model = _mapper.Map<PeriodModel>(request);
        await _validator.ValidateAsync(model, cancellationToken).ThrowIfInvalidAsync();
        await _periodsRepository.OpenAsync(model.Year, model.Month, cancellationToken);
    }
}
