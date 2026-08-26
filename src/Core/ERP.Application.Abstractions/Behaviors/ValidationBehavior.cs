using ERP.Domain.Exceptions;
using FluentValidation;
using MediatR;

namespace ERP.Application.Abstractions.Behaviors;

/// <summary>
/// Pipeline behavior that validates requests using FluentValidation validators
/// before they reach the handler. Automatically applied to all MediatR requests
/// that have registered validators.
/// </summary>
/// <typeparam name="TRequest">The type of the request</typeparam>
/// <typeparam name="TResponse">The type of the response</typeparam>
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);
        
        var validationTasks = _validators
            .Select(validator => validator.ValidateAsync(context, cancellationToken));
        
        var validationResults = await Task.WhenAll(validationTasks);
        
        var failures = validationResults
            .SelectMany(result => result.Errors)
            .Where(error => error is not null)
            .ToList();

        if (failures.Count > 0)
        {
            var validationErrors = failures
                .GroupBy(x => x.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.ErrorMessage).ToArray()
                );

            throw new ERP.Domain.Exceptions.ValidationException("Validation failed", validationErrors);
        }

        return await next();
    }
}