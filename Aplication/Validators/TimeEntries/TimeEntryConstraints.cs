using Domain.Models;

namespace Application.Validators.TimeEntries;

internal static class TimeEntryConstraints
{
    public const decimal MaxHoursPerDay = 24m;
    public const decimal OvertimeThreshold = 12m;
    public const decimal Step = 0.5m;

    public static bool IsValidEntryHours(decimal hours) =>
        hours > 0
        && hours <= MaxHoursPerDay
        && hours / Step == decimal.Truncate(hours / Step);

    public static decimal? FindRate(IEnumerable<RateModel> rates, DateOnly date) =>
        rates
            .Where(rate => rate.From <= date)
            .OrderByDescending(rate => rate.From)
            .Select(rate => (decimal?)rate.Value)
            .FirstOrDefault();

    public static bool IsOvertime(decimal hoursForDay) =>
        hoursForDay > OvertimeThreshold;
}
