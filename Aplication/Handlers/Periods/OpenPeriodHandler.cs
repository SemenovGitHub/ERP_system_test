using Application.Interfaces;
using Application.Models.Periods.Commands;
using MediatR;

namespace Application.Handlers.Periods;

public sealed class OpenPeriodHandler : IRequestHandler<OpenPeriodCommand>
{
    private readonly IPeriodRepository _periods;

    public OpenPeriodHandler(IPeriodRepository periods)
    {
        _periods = periods;
    }

    public Task Handle(OpenPeriodCommand request, CancellationToken cancellationToken) =>
        _periods.OpenAsync(request.Year, request.Month, cancellationToken);
}
