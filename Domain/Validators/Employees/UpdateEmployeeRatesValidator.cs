using Domain.Models;
using FluentValidation;
using FluentValidation.Results;

namespace Domain.Validators.Employees;

public sealed class UpdateEmployeeRatesValidator
    : AbstractValidator<EmployeeModel>,
      IDomainValidator<EmployeeModel>
{
    public UpdateEmployeeRatesValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Rates)
            .NotEmpty()
            .WithMessage("Нужна хотя бы одна ставка с положительным значением.");
        RuleForEach(x => x.Rates).ChildRules(rate =>
        {
            rate.RuleFor(item => item.From).NotEqual(default(DateOnly));
            rate.RuleFor(item => item.Value)
                .GreaterThan(0)
                .WithMessage("Нужна хотя бы одна ставка с положительным значением.");
        });
    }

    async Task IDomainValidator<EmployeeModel>.ValidateAsync(
        EmployeeModel instance,
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
