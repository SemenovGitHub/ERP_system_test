using Application.Models.TimeEntries.Responses;
using MediatR;

namespace Application.Models.TimeEntries.Queries;

public sealed class GetTimeEntriesQuery : IRequest<PagedTimeEntriesResponse>
{
    public int Year { get; set; }

    public int Month { get; set; }

    public Guid? EmployeeId { get; set; }

    public Guid? ProjectId { get; set; }

    public int Page { get; set; }

    public int PageSize { get; set; }
}
