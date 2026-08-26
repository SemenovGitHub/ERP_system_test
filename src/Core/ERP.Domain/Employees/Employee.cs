namespace ERP.Domain.Employees;

public sealed class Employee
{
    public Guid Id { get; init; }

    public string FullName { get; init; } = null!;

    public string Department { get; init; } = null!;

    public IReadOnlyList<Rate> Rates { get; init; } = [];
}
