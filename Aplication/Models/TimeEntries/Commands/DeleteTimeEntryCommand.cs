using MediatR;

namespace Application.Models.TimeEntries.Commands;

public sealed class DeleteTimeEntryCommand : IRequest
{
    public Guid Id { get; set; }
}
