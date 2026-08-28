using Application.Models.TimeEntries.Responses;
using Domain.Models;

namespace Application.Mapping;

internal sealed record TimeEntryMapSource(
    TimeEntryModel Entry,
    EmployeeModel Employee,
    ProjectModel Project,
    decimal HoursForDay);
