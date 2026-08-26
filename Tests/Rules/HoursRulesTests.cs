using Domain.Exceptions;
using Domain.Rules;
using FluentAssertions;

namespace ERP.Tests.Rules;

public sealed class HoursRulesTests
{
    [Fact]
    public void EnsureDailyLimit_WhenTotalEquals24_DoesNotThrow()
    {
        var act = () => HoursRules.EnsureDailyLimit(16, 8);

        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureDailyLimit_WhenTotalExceeds24_ThrowsDailyHoursLimit()
    {
        var act = () => HoursRules.EnsureDailyLimit(16, 10);

        var exception = act.Should().Throw<BusinessException>().Which;
        exception.Code.Should().Be(ErrorCodes.DailyHoursLimit);
        exception.Message.Should().Be(
            "Суммарно у сотрудника за день не может быть больше 24 часов. Уже учтено 16, попытка добавить 10 (итого 26).");
    }
}
