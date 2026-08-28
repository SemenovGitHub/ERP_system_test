using AutoMapper;
using Domain.Models;
using Domain.Validators;
using Domain.Validators.Employees;
using Domain.Validators.Periods;
using Domain.Validators.TimeEntries;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
        });

        services.AddAutoMapper(typeof(AutoMapperProfile));

        services.AddTransient<ICreateTimeEntryValidator, CreateTimeEntryValidator>();
        services.AddTransient<IUpdateTimeEntryValidator, UpdateTimeEntryValidator>();
        services.AddTransient<IDeleteTimeEntryValidator, DeleteTimeEntryValidator>();
        services.AddTransient<IDomainValidator<EmployeeModel>, UpdateEmployeeRatesValidator>();
        services.AddTransient<IDomainValidator<PeriodModel>, PeriodValidator>();

        return services;
    }
}
