using Domain.Models;
using Domain.Validators.Employees;
using Domain.Validators.Periods;
using Domain.Validators.TimeEntries;
using FluentValidation;
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

        services.AddTransient<ITimeEntryValidator, CreateTimeEntryValidator>();
        services.AddTransient<ITimeEntryValidator, UpdateTimeEntryValidator>();
        services.AddTransient<ITimeEntryValidator, DeleteTimeEntryValidator>();
        services.AddTransient<IValidator<EmployeeModel>, UpdateEmployeeRatesValidator>();
        services.AddTransient<IValidator<PeriodModel>, PeriodValidator>();

        return services;
    }
}
