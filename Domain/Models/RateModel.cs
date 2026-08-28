namespace Domain.Models;

public sealed class RateModel
{
    public DateOnly From { get; init; }

    public decimal Value { get; init; }
}
