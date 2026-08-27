using Application.Handlers.TimeEntries;
using Application.Models.Employees.Commands;
using Application.Models.Periods.Commands;
using Application.Models.TimeEntries.Commands;
using Application.Validators;
using Application.Validators.Employees;
using Application.Validators.Periods;
using Application.Validators.TimeEntries;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(typeof(CreateTimeEntryHandler).Assembly);
        });

        services.AddTransient<IDomainValidator<CreateTimeEntryCommand>, CreateTimeEntryValidator>();
        services.AddTransient<IDomainValidator<UpdateTimeEntryCommand>, UpdateTimeEntryValidator>();
        services.AddTransient<IDomainValidator<DeleteTimeEntryCommand>, DeleteTimeEntryValidator>();
        services.AddTransient<IDomainValidator<UpdateEmployeeRatesCommand>, UpdateEmployeeRatesValidator>();
        services.AddTransient<IDomainValidator<ClosePeriodCommand>, ClosePeriodValidator>();
        services.AddTransient<IDomainValidator<OpenPeriodCommand>, OpenPeriodValidator>();

        return services;
    }
}
