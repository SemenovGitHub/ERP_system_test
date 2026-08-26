using Domain.Exceptions;

namespace Domain.Rules;

public static class ClosedPeriodRules
{
    public static void EnsureOpen(bool isClosed, DateOnly date)
    {
        if (isClosed)
        {
            throw new BusinessException(
                ErrorCodes.ClosedPeriod,
                $"Период {date.Month:00}.{date.Year} закрыт. Создавать, изменять и удалять записи нельзя.",
                statusCode: 409);
        }
    }
}
