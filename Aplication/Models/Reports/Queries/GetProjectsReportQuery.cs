using Application.Models.Reports.Responses;
using MediatR;

namespace Application.Models.Reports.Queries;

public sealed class GetProjectsReportQuery : IRequest<ProjectReportResponse>
{
    public int Year { get; set; }

    public int Month { get; set; }
}
