using Domain.Exceptions;
using Domain.Rules;
using FluentAssertions;

namespace ERP.Tests.Rules;

public sealed class ClosedPeriodRulesTests
{
    [Fact]
    public void EnsureOpen_WhenPeriodIsOpen_DoesNotThrow()
    {
        var act = () => ClosedPeriodRules.EnsureOpen(false, new DateOnly(2026, 3, 15));

        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureOpen_WhenPeriodIsClosed_ThrowsClosedPeriod()
    {
        var act = () => ClosedPeriodRules.EnsureOpen(true, new DateOnly(2026, 3, 15));

        var exception = act.Should().Throw<BusinessException>().Which;
        exception.Code.Should().Be(ErrorCodes.ClosedPeriod);
        exception.StatusCode.Should().Be(409);
        exception.Message.Should().Be("Период 03.2026 закрыт. Создавать, изменять и удалять записи нельзя.");
    }
}
