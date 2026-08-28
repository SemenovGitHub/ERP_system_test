using Application.Models.Periods.Commands;
using AutoMapper;
using Domain.Interfaces;
using Domain.Models;
using Domain.Validators;
using MediatR;

namespace Application.Handlers.Periods;

public sealed class ClosePeriodHandler : IRequestHandler<ClosePeriodCommand>
{
    private readonly IPeriodRepository _periods;
    private readonly IDomainValidator<PeriodModel> _validator;
    private readonly IMapper _mapper;

    public ClosePeriodHandler(
        IPeriodRepository periods,
        IDomainValidator<PeriodModel> validator,
        IMapper mapper)
    {
        _periods = periods;
        _validator = validator;
        _mapper = mapper;
    }

    public async Task Handle(ClosePeriodCommand request, CancellationToken cancellationToken)
    {
        var model = _mapper.Map<PeriodModel>(request);
        await _validator.ValidateAsync(model, cancellationToken);
        await _periods.CloseAsync(model.Year, model.Month, cancellationToken);
    }
}
