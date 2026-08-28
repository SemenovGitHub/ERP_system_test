using Api.Abstractions.Models.Employees;
using Application.Models.Employees.Commands;
using Application.Models.Employees.Queries;
using AutoMapper;
using ERP.Abstractions.Models.Employees;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/employees")]
public class EmployeesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public EmployeesController(
        IMediator mediator,
        IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<PagedEmployeesDto>> Get(
        [FromQuery] GetEmployeesDto dto,
        CancellationToken cancellationToken)
    {
        var query = _mapper.Map<GetEmployeesQuery>(dto);

        var result = await _mediator.Send(
            query,
            cancellationToken);

        var response = _mapper.Map<PagedEmployeesDto>(result);

        return Ok(response);
    }

    [HttpPut("{id:guid}/rates")]
    public async Task<ActionResult<EmployeeDto>> UpdateRates(
        Guid id,
        [FromBody] UpdateEmployeeRatesDto dto,
        CancellationToken cancellationToken)
    {
        var command = _mapper.Map<UpdateEmployeeRatesCommand>(dto);
        command.Id = id;

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(_mapper.Map<EmployeeDto>(result));
    }
}
