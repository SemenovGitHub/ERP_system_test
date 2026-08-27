using Application.Models.Periods.Commands;
using FluentValidation;
using FluentValidation.Results;

namespace Application.Validators.Periods;

public sealed class OpenPeriodValidator
    : AbstractValidator<OpenPeriodCommand>,
      IDomainValidator<OpenPeriodCommand>
{
    public OpenPeriodValidator()
    {
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100);
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
    }

    async Task IDomainValidator<OpenPeriodCommand>.ValidateAsync(
        OpenPeriodCommand instance,
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
