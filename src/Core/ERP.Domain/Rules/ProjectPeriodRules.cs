using ERP.Domain.Exceptions;
using ERP.Domain.Projects;

namespace ERP.Domain.Rules;

public static class ProjectPeriodRules
{
    public static void EnsureDateFits(Project project, DateOnly date)
    {
        if (date < project.StartDate)
        {
            throw new BusinessException(
                ErrorCodes.ProjectDateOutOfRange,
                $"Дата записи {date:dd.MM.yyyy} раньше начала проекта {project.Code} ({project.StartDate:dd.MM.yyyy}).");
        }

        if (project.EndDate is { } end && date > end)
        {
            throw new BusinessException(
                ErrorCodes.ProjectDateOutOfRange,
                $"Дата записи {date:dd.MM.yyyy} позже окончания проекта {project.Code} ({end:dd.MM.yyyy}).");
        }
    }
}
