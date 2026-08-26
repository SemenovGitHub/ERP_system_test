using ERP.Application.Abstractions.Repositories;
using ERP.Persistence.Configuration;
using ERP.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ERP.Persistence;

/// <summary>
/// Extension methods for configuring persistence services
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds persistence services to the service collection
    /// </summary>
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure MongoDB settings
        services.Configure<MongoDbSettings>(configuration.GetSection(MongoDbSettings.SectionName));
        
        // Validate configuration at startup
        services.AddSingleton<IValidateOptions<MongoDbSettings>, ValidateMongoDbSettings>();

        // Register repositories
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<ITimeEntryRepository, TimeEntryRepository>();

        return services;
    }
}

/// <summary>
/// Validates MongoDb settings configuration
/// </summary>
internal sealed class ValidateMongoDbSettings : IValidateOptions<MongoDbSettings>
{
    public ValidateOptionsResult Validate(string? name, MongoDbSettings options)
    {
        try
        {
            options.Validate();
            return ValidateOptionsResult.Success;
        }
        catch (ArgumentException ex)
        {
            return ValidateOptionsResult.Fail(ex.Message);
        }
    }
}