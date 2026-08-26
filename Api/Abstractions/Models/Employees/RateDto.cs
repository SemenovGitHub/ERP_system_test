namespace ERP.Abstractions.Models.Employees;

public sealed class RateDto
{
    public DateOnly From { get; set; }

    public decimal Value { get; set; }
}
