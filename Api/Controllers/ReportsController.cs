using Application.Models.Reports.Queries;
using AutoMapper;
using ERP.Abstractions.Models.Reports;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public ReportsController(
        IMediator mediator,
        IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    [HttpGet("projects")]
    public async Task<ActionResult<ProjectReportDto>> GetProjects(
        [FromQuery] GetProjectsReportDto dto,
        CancellationToken cancellationToken)
    {
        var query = _mapper.Map<GetProjectsReportQuery>(dto);

        var result = await _mediator.Send(
            query,
            cancellationToken);

        var response = _mapper.Map<ProjectReportDto>(result);

        return Ok(response);
    }
}
