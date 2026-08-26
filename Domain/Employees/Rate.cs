namespace Domain.Employees;

public sealed class Rate
{
    public DateOnly From { get; init; }

    public decimal Value { get; init; }
}
