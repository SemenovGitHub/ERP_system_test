namespace Application.Interfaces;

public interface IProjectReportRepository
{
    Task<IReadOnlyList<ProjectReportRow>> GetByMonthAsync(
        int year,
        int month,
        CancellationToken cancellationToken);
}
