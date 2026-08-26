using Application.Behaviors;
using Application.Handlers.TimeEntries;
using Application.Validators.TimeEntries;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(typeof(CreateTimeEntryHandler).Assembly);
            // FluentValidation runs here, before any handler. See Application.Behaviors.ValidationBehavior.
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(CreateTimeEntryCommandValidator).Assembly);

        return services;
    }
}
