using Application.Models.Periods.Commands;
using AutoMapper;
using ERP.Abstractions.Models.Periods;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/periods")]
public class PeriodsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public PeriodsController(
        IMediator mediator,
        IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    [HttpPost("close")]
    public async Task<IActionResult> Close(
        [FromBody] PeriodDto dto,
        CancellationToken cancellationToken)
    {
        var command = _mapper.Map<ClosePeriodCommand>(dto);

        await _mediator.Send(
            command,
            cancellationToken);

        return NoContent();
    }

    [HttpPost("open")]
    public async Task<IActionResult> Open(
        [FromBody] PeriodDto dto,
        CancellationToken cancellationToken)
    {
        var command = _mapper.Map<OpenPeriodCommand>(dto);

        await _mediator.Send(
            command,
            cancellationToken);

        return NoContent();
    }
}
