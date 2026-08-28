using Domain.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Repository.Mongo;
using Repository.Repositories;

namespace Repository;

public static class DependencyInjection
{
    public static IServiceCollection AddMongoRepositories(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        MongoSerializers.Register();

        services.AddAutoMapper(typeof(AutoMapperProfile));

        services.AddOptions<MongoSettings>()
            .Bind(configuration.GetSection(MongoSettings.SectionName));

        services.AddSingleton<IMongoClient>(provider =>
        {
            var settings = provider.GetRequiredService<IOptions<MongoSettings>>().Value;
            return new MongoClient(settings.ConnectionString);
        });

        services.AddSingleton(provider =>
        {
            var settings = provider.GetRequiredService<IOptions<MongoSettings>>().Value;
            return provider.GetRequiredService<IMongoClient>().GetDatabase(settings.DatabaseName);
        });

        services.AddSingleton<MongoCollections>();
        services.AddSingleton<MongoIndexInitializer>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IPeriodRepository, PeriodRepository>();
        services.AddScoped<ITimeEntryRepository, TimeEntryRepository>();
        services.AddScoped<IProjectReportRepository, ProjectReportRepository>();

        return services;
    }
}
