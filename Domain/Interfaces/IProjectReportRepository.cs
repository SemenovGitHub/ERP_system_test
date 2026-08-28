using Domain.Models;

namespace Domain.Interfaces;

public interface IProjectReportRepository
{
    Task<IReadOnlyList<ProjectReportModel>> GetByMonthAsync(
        int year,
        int month,
        CancellationToken cancellationToken);
}
