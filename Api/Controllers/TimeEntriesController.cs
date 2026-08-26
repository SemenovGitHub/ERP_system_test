using AutoMapper;
using ERP.Abstractions.Models.TimeEntries;
using MediatR;
using Application.Models.TimeEntries.Commands;
using Application.Models.TimeEntries.Queries;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Controllers;

[ApiController]
[Route("api/time-entries")]
public class TimeEntriesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public TimeEntriesController(
        IMediator mediator,
        IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<PagedTimeEntriesDto>> Get(
        [FromQuery] GetTimeEntriesDto dto,
        CancellationToken cancellationToken)
    {
        var query = _mapper.Map<GetTimeEntriesQuery>(dto);

        var result = await _mediator.Send(
            query,
            cancellationToken);

        var response = _mapper.Map<PagedTimeEntriesDto>(result);

        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<TimeEntryDto>> Create(
        [FromBody] CreateTimeEntryDto dto,
        CancellationToken cancellationToken)
    {
        var command = _mapper.Map<CreateTimeEntryCommand>(dto);

        var result = await _mediator.Send(
            command,
            cancellationToken);

        var response = _mapper.Map<TimeEntryDto>(result);

        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TimeEntryDto>> Update(
        Guid id,
        [FromBody] UpdateTimeEntryDto dto,
        CancellationToken cancellationToken)
    {
        var command = _mapper.Map<UpdateTimeEntryCommand>(dto);

        command.Id = id;

        var result = await _mediator.Send(
            command,
            cancellationToken);

        var response = _mapper.Map<TimeEntryDto>(result);

        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteTimeEntryCommand
        {
            Id = id
        };

        await _mediator.Send(
            command,
            cancellationToken);

        return NoContent();
    }
}