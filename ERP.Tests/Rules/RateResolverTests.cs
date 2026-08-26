using Domain.Employees;
using Domain.Exceptions;
using Domain.Rules;
using FluentAssertions;

namespace ERP.Tests.Rules;

public sealed class RateResolverTests
{
    [Fact]
    public void Require_WhenSeveralRatesExist_PicksLatestRateNotLaterThanDate()
    {
        var rates = new[]
        {
            new Rate { From = new DateOnly(2026, 1, 1), Value = 500 },
            new Rate { From = new DateOnly(2026, 3, 1), Value = 700 },
            new Rate { From = new DateOnly(2026, 6, 1), Value = 900 }
        };

        var rate = RateResolver.Require(rates, new DateOnly(2026, 4, 15));

        rate.Should().Be(700);
    }

    [Fact]
    public void Require_WhenDateEqualsRateFrom_UsesThatRate()
    {
        var rates = new[]
        {
            new Rate { From = new DateOnly(2026, 1, 1), Value = 500 },
            new Rate { From = new DateOnly(2026, 3, 1), Value = 700 }
        };

        RateResolver.Require(rates, new DateOnly(2026, 3, 1)).Should().Be(700);
    }

    [Fact]
    public void Require_WhenNoRateCoversDate_ThrowsNoRate()
    {
        var rates = new[]
        {
            new Rate { From = new DateOnly(2026, 3, 1), Value = 700 }
        };

        var act = () => RateResolver.Require(rates, new DateOnly(2026, 2, 28));

        var exception = act.Should().Throw<BusinessException>().Which;
        exception.Code.Should().Be(ErrorCodes.NoRate);
        exception.Message.Should().Be("На дату записи у сотрудника нет ни одной ставки. Запись создать нельзя.");
    }
}
