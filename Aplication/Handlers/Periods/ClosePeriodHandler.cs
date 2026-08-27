using Application.Interfaces;
using Application.Models.Periods.Commands;
using Application.Validators;
using MediatR;

namespace Application.Handlers.Periods;

public sealed class ClosePeriodHandler : IRequestHandler<ClosePeriodCommand>
{
    private readonly IPeriodRepository _periods;
    private readonly IDomainValidator<ClosePeriodCommand> _validator;

    public ClosePeriodHandler(
        IPeriodRepository periods,
        IDomainValidator<ClosePeriodCommand> validator)
    {
        _periods = periods;
        _validator = validator;
    }

    public async Task Handle(ClosePeriodCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAsync(request, cancellationToken);
        await _periods.CloseAsync(request.Year, request.Month, cancellationToken);
    }
}
