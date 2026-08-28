namespace Domain.Models;

public sealed class EmployeeModel
{
    public Guid Id { get; init; }

    public string FullName { get; init; } = null!;

    public string Department { get; init; } = null!;

    public IReadOnlyList<RateModel> Rates { get; init; } = [];
}
