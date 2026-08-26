using AutoMapper;
using ERP.Abstractions.Models.Projects;
using MediatR;
using Application.Models.Projects.Queries;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Controllers;

[ApiController]
[Route("api/projects")]
public class ProjectsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public ProjectsController(
        IMediator mediator,
        IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<PagedProjectsDto>> Get(
        [FromQuery] GetProjectsDto dto,
        CancellationToken cancellationToken)
    {
        var query = _mapper.Map<GetProjectsQuery>(dto);

        var result = await _mediator.Send(
            query,
            cancellationToken);

        var response = _mapper.Map<PagedProjectsDto>(result);

        return Ok(response);
    }
}