using Domain.Exceptions;
using FluentValidation.Results;

namespace Domain.Validators;

public static class ValidationResultExtensions
{
    public static async Task ThrowIfInvalidAsync(this Task<ValidationResult> validation)
    {
        var result = await validation;
        result.ThrowIfInvalid();
    }

    public static void ThrowIfInvalid(this ValidationResult result)
    {
        if (result.IsValid)
        {
            return;
        }

        var business = result.Errors.FirstOrDefault(error =>
            error.ErrorCode is
                ErrorCodes.NoRate or
                ErrorCodes.DailyHoursLimit or
                ErrorCodes.ClosedPeriod or
                ErrorCodes.ProjectDateOutOfRange or
                ErrorCodes.InvalidHours or
                ErrorCodes.NotFound);

        if (business is not null)
        {
            var status = business.ErrorCode switch
            {
                ErrorCodes.ClosedPeriod => 409,
                ErrorCodes.NotFound => 404,
                _ => 400
            };

            throw new BusinessException(business.ErrorCode, business.ErrorMessage, status);
        }

        var errors = result.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.ErrorMessage).Distinct().ToArray());

        throw new Domain.Exceptions.ValidationException("Ошибка валидации запроса.", errors);
    }
}
