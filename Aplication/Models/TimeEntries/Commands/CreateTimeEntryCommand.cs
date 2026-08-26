using Application.Models.TimeEntries.Responses;
using MediatR;

namespace Application.Models.TimeEntries.Commands;

public sealed class CreateTimeEntryCommand : IRequest<TimeEntryResponse>
{
    public Guid EmployeeId { get; set; }

    public Guid ProjectId { get; set; }

    public DateOnly Date { get; set; }

    public decimal Hours { get; set; }

    public string? Comment { get; set; }
}
