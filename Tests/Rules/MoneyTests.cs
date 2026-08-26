using Domain.Rules;
using FluentAssertions;

namespace ERP.Tests.Rules;

public sealed class MoneyTests
{
    [Theory]
    [InlineData(1.224, 1.22)]
    [InlineData(1.225, 1.23)]
    [InlineData(1.235, 1.24)]
    public void Round_ToTwoDecimals_UsesAwayFromZero(decimal value, decimal expected)
    {
        Money.Round(value).Should().Be(expected);
    }

    [Fact]
    public void Cost_MultipliesHoursByRateAndRoundsAwayFromZero()
    {
        Money.Cost(1.5m, 10.005m).Should().Be(15.01m);
        Money.Cost(7.5m, 600m).Should().Be(4500m);
    }
}
