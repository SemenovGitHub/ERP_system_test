using ERP.Application.Abstractions.Behaviors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace ERP.Infrastructure;

/// <summary>
/// Extension methods for configuring infrastructure services
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds infrastructure services to the service collection
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Register MediatR handlers from this assembly
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            
            // Pipeline behaviors are registered in Application.Abstractions
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        // Register FluentValidation validators from this assembly
        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        return services;
    }
}