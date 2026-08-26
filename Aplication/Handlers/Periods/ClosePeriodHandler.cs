using Application.Interfaces;
using Application.Models.Periods.Commands;
using MediatR;

namespace Application.Handlers.Periods;

public sealed class ClosePeriodHandler : IRequestHandler<ClosePeriodCommand>
{
    private readonly IPeriodRepository _periods;

    public ClosePeriodHandler(IPeriodRepository periods)
    {
        _periods = periods;
    }

    public Task Handle(ClosePeriodCommand request, CancellationToken cancellationToken) =>
        _periods.CloseAsync(request.Year, request.Month, cancellationToken);
}
