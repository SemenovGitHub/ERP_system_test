namespace Application.Models.Employees.Responses;

public sealed class EmployeeResponse
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = null!;

    public string Department { get; set; } = null!;

    public IReadOnlyCollection<RateResponse> Rates { get; set; } = [];
}
