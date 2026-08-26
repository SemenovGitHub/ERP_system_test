using MediatR;

namespace Application.Models.Periods.Commands;

public sealed class ClosePeriodCommand : IRequest
{
    public int Year { get; set; }

    public int Month { get; set; }
}
