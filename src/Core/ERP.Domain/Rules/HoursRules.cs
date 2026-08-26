using ERP.Domain.Exceptions;

namespace ERP.Domain.Rules;

public static class HoursRules
{
    public const decimal MaxHoursPerDay = 24m;
    public const decimal OvertimeThreshold = 12m;
    public const decimal Step = 0.5m;

    public static bool IsValidEntryHours(decimal hours) =>
        hours > 0
        && hours <= MaxHoursPerDay
        && hours / Step == decimal.Truncate(hours / Step);

    public static void EnsureValidEntryHours(decimal hours)
    {
        if (!IsValidEntryHours(hours))
        {
            throw new BusinessException(
                ErrorCodes.InvalidHours,
                "Часы должны быть положительными, кратными 0,5 и не больше 24 за одну запись.");
        }
    }

    public static void EnsureDailyLimit(decimal hoursAlreadyLogged, decimal hoursToAdd)
    {
        var total = hoursAlreadyLogged + hoursToAdd;

        if (total > MaxHoursPerDay)
        {
            throw new BusinessException(
                ErrorCodes.DailyHoursLimit,
                $"Суммарно у сотрудника за день не может быть больше {MaxHoursPerDay} часов. " +
                $"Уже учтено {hoursAlreadyLogged}, попытка добавить {hoursToAdd} (итого {total}).");
        }
    }

    public static bool IsOvertime(decimal hoursForDay) =>
        hoursForDay > OvertimeThreshold;
}
