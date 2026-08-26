using ERP.Domain.Employees;
using ERP.Domain.Exceptions;

namespace ERP.Domain.Rules;

public static class RateResolver
{
    public static decimal? Find(IEnumerable<Rate> rates, DateOnly date)
    {
        return rates
            .Where(rate => rate.From <= date)
            .OrderByDescending(rate => rate.From)
            .Select(rate => (decimal?)rate.Value)
            .FirstOrDefault();
    }

    public static decimal Require(IEnumerable<Rate> rates, DateOnly date)
    {
        var rate = Find(rates, date);

        if (rate is null)
        {
            throw new BusinessException(
                ErrorCodes.NoRate,
                "На дату записи у сотрудника нет ни одной ставки. Запись создать нельзя.");
        }

        return rate.Value;
    }
}
