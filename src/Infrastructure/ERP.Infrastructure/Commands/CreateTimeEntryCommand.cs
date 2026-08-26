using ERP.Application.Abstractions.Commands;

namespace ERP.Infrastructure.Commands;

/// <summary>
/// Command to create a new time entry
/// </summary>
public sealed record CreateTimeEntryCommand(
    Guid EmployeeId,
    Guid ProjectId,
    DateOnly Date,
    decimal Hours,
    string? Comment = null
) : ICommand<CreateTimeEntryResponse>;

public sealed record CreateTimeEntryResponse(
    Guid Id,
    Guid EmployeeId,
    string EmployeeFullName,
    Guid ProjectId,
    string ProjectCode,
    DateOnly Date,
    decimal Hours,
    string? Comment,
    decimal Rate,
    decimal Cost,
    bool IsOvertime,
    decimal TotalHoursForDay
);