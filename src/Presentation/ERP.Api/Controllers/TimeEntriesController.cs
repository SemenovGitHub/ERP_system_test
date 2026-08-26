using ERP.Infrastructure.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

/// <summary>
/// Time entries management controller
/// 
/// Error Handling Flow:
/// 1. Model validation (ASP.NET Core model binding)
/// 2. ValidationBehavior (FluentValidation) 
/// 3. Business logic validation (in handlers)
/// 4. GlobalExceptionMiddleware (converts exceptions to HTTP responses)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TimeEntriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public TimeEntriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create a new time entry
    /// </summary>
    /// <param name="request">Time entry data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created time entry</returns>
    /// <response code="201">Time entry created successfully</response>
    /// <response code="400">Validation error or business rule violation</response>
    /// <response code="404">Employee or project not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(CreateTimeEntryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(
        [FromBody] CreateTimeEntryCommand request,
        CancellationToken cancellationToken)
    {
        /* 
         * Validation and Error Handling Flow:
         * 
         * 1. ASP.NET Core Model Binding validates JSON structure
         * 2. ValidationBehavior runs CreateTimeEntryCommandValidator
         *    - Validates required fields (EmployeeId, ProjectId, Date)
         *    - Validates hours format (positive, divisible by 0.5, ≤ 24)
         *    - Validates comment length (≤ 500 chars)
         *    - Throws ValidationException if validation fails → HTTP 400
         * 
         * 3. CreateTimeEntryHandler runs business validation:
         *    - Checks employee exists → BusinessException "NOT_FOUND" → HTTP 400
         *    - Checks project exists → BusinessException "NOT_FOUND" → HTTP 400  
         *    - Validates project date range → BusinessException "PROJECT_DATE_OUT_OF_RANGE" → HTTP 400
         *    - Validates employee has rate for date → BusinessException "RATE_NOT_FOUND" → HTTP 400
         *    - Validates daily hours limit → BusinessException "DAILY_HOURS_LIMIT" → HTTP 400
         * 
         * 4. GlobalExceptionMiddleware converts all exceptions to structured JSON responses
         * 
         * Example Error Responses:
         * 
         * ValidationException (FluentValidation):
         * {
         *   "code": "VALIDATION_ERROR",
         *   "message": "One or more validation errors occurred",
         *   "validationErrors": {
         *     "Hours": ["Часы должны быть положительными, кратными 0,5 и не больше 24"],
         *     "EmployeeId": ["ID сотрудника обязателен"]
         *   }
         * }
         * 
         * BusinessException (Business Rules):
         * {
         *   "code": "DAILY_HOURS_LIMIT",
         *   "message": "Суммарно у сотрудника за день не может быть больше 24 часов. Уже учтено 16, попытка добавить 10 (итого 26)."
         * }
         */
        
        var result = await _mediator.Send(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Get time entry by ID (placeholder)
    /// </summary>
    [HttpGet("{id:guid}")]
    public IActionResult GetById(Guid id)
    {
        // TODO: Implement GetTimeEntryByIdQuery
        return Ok(new { id, message = "Time entry details - to be implemented" });
    }
}

/// <summary>
/// Error response format used by GlobalExceptionMiddleware
/// </summary>
public record ErrorResponse(
    string Code,
    string Message,
    IReadOnlyDictionary<string, string[]>? ValidationErrors = null
);