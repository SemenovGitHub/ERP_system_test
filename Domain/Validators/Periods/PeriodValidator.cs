using Domain.Models;
using FluentValidation;
using FluentValidation.Results;

namespace Domain.Validators.Periods;

public sealed class PeriodValidator
    : AbstractValidator<PeriodModel>,
      IDomainValidator<PeriodModel>
{
    public PeriodValidator()
    {
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100);
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
    }

    async Task IDomainValidator<PeriodModel>.ValidateAsync(
        PeriodModel instance,
        CancellationToken cancellationToken)
    {
        var result = await base.ValidateAsync(instance, cancellationToken);
        ThrowIfInvalid(result);
    }

    private static void ThrowIfInvalid(ValidationResult result)
    {
        if (result.IsValid)
        {
            return;
        }

        var errors = result.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.ErrorMessage).Distinct().ToArray());

        throw new Domain.Exceptions.ValidationException("Ошибка валидации запроса.", errors);
    }
}
