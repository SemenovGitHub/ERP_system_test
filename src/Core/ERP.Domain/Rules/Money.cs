namespace ERP.Domain.Rules;

public static class Money
{
    public static decimal Round(decimal value) =>
        Math.Round(value, 2, MidpointRounding.AwayFromZero);

    public static decimal Cost(decimal hours, decimal rate) =>
        Round(hours * rate);
}
