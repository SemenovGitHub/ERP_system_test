namespace ERP.Abstractions.Models.Employees;

public sealed class EmployeeDto
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = null!;
}