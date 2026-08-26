namespace Application.Models.Employees.Responses;

public sealed class RateResponse
{
    public DateOnly From { get; set; }

    public decimal Value { get; set; }
}
