namespace Domain.Models;

public sealed class ProjectModel
{
    public Guid Id { get; init; }

    public string Code { get; init; } = null!;

    public string Name { get; init; } = null!;

    public decimal Budget { get; init; }

    public DateOnly StartDate { get; init; }

    public DateOnly? EndDate { get; init; }
}
