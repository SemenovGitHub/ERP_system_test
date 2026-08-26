using Application.Models.Employees.Commands;
using FluentValidation;

namespace Application.Validators.Employees;

public sealed class UpdateEmployeeRatesCommandValidator : AbstractValidator<UpdateEmployeeRatesCommand>
{
    public UpdateEmployeeRatesCommandValidator()
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
}
