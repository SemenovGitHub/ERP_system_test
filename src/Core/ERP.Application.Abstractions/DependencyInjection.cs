using ERP.Application.Abstractions.Behaviors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace ERP.Application.Abstractions;

/// <summary>
/// Extension methods for configuring application services
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds application abstraction services to the service collection
    /// </summary>
    public static IServiceCollection AddApplicationAbstractions(this IServiceCollection services)
    {
        // Register MediatR with all handlers from calling assembly
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetCallingAssembly());
            
            // Add pipeline behaviors
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        // Register FluentValidation validators from calling assembly
        services.AddValidatorsFromAssembly(Assembly.GetCallingAssembly(), includeInternalTypes: true);

        return services;
    }
}