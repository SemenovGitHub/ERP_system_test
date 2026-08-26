namespace Domain.Exceptions;

public static class ErrorCodes
{
    public const string NoRate = "NO_RATE";
    public const string DailyHoursLimit = "DAILY_HOURS_LIMIT";
    public const string ClosedPeriod = "CLOSED_PERIOD";
    public const string ProjectDateOutOfRange = "PROJECT_DATE_OUT_OF_RANGE";
    public const string InvalidHours = "INVALID_HOURS";
    public const string ConcurrencyConflict = "CONCURRENCY_CONFLICT";
    public const string NotFound = "NOT_FOUND";
    public const string Validation = "VALIDATION_ERROR";
}
