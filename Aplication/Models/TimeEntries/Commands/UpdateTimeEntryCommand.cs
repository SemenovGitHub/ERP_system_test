using Application.Models.TimeEntries.Responses;
using MediatR;

namespace Application.Models.TimeEntries.Commands;

public sealed class UpdateTimeEntryCommand : IRequest<TimeEntryResponse>
{
    public Guid Id { get; set; }

    public Guid EmployeeId { get; set; }

    public Guid ProjectId { get; set; }

    public DateOnly Date { get; set; }

    public decimal Hours { get; set; }

    public string? Comment { get; set; }

    public int Version { get; set; }
}
