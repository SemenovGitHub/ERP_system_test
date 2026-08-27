using Application.Interfaces;
using Application.Models.Periods.Commands;
using Application.Validators;
using MediatR;

namespace Application.Handlers.Periods;

public sealed class OpenPeriodHandler : IRequestHandler<OpenPeriodCommand>
{
    private readonly IPeriodRepository _periods;
    private readonly IDomainValidator<OpenPeriodCommand> _validator;

    public OpenPeriodHandler(
        IPeriodRepository periods,
        IDomainValidator<OpenPeriodCommand> validator)
    {
        _periods = periods;
        _validator = validator;
    }

    public async Task Handle(OpenPeriodCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAsync(request, cancellationToken);
        await _periods.OpenAsync(request.Year, request.Month, cancellationToken);
    }
}
