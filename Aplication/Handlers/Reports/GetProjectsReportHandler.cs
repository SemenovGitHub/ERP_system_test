using Application.Models.Reports.Queries;
using Application.Models.Reports.Responses;
using AutoMapper;
using Domain.Interfaces;
using MediatR;

namespace Application.Handlers.Reports;

public sealed class GetProjectsReportHandler
    : IRequestHandler<GetProjectsReportQuery, ProjectReportResponse>
{
    private readonly IProjectReportRepository _reports;
    private readonly IMapper _mapper;

    public GetProjectsReportHandler(IProjectReportRepository reports, IMapper mapper)
    {
        _reports = reports;
        _mapper = mapper;
    }

    public async Task<ProjectReportResponse> Handle(
        GetProjectsReportQuery request,
        CancellationToken cancellationToken)
    {
        var rows = await _reports.GetByMonthAsync(request.Year, request.Month, cancellationToken);
        return _mapper.Map<ProjectReportResponse>(rows);
    }
}
