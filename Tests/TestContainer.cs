using Application.Interfaces;
using Application.Models.TimeEntries.Commands;
using Application.Validators;
using Application.Validators.TimeEntries;
using Autofac;
using Moq;

namespace Tests;

internal static class TestContainer
{
    public static IDomainValidator<TCommand> Resolve<TCommand>(
        Mock<IEmployeeRepository>? employeeRepositoryMock = null,
        Mock<IProjectRepository>? projectRepositoryMock = null,
        Mock<IPeriodRepository>? periodRepositoryMock = null,
        Mock<ITimeEntryRepository>? timeEntryRepositoryMock = null)
    {
        var builder = new ContainerBuilder();
        builder.RegisterInstance((employeeRepositoryMock ?? new Mock<IEmployeeRepository>()).Object).As<IEmployeeRepository>();
        builder.RegisterInstance((projectRepositoryMock ?? new Mock<IProjectRepository>()).Object).As<IProjectRepository>();
        builder.RegisterInstance((periodRepositoryMock ?? new Mock<IPeriodRepository>()).Object).As<IPeriodRepository>();
        builder.RegisterInstance((timeEntryRepositoryMock ?? new Mock<ITimeEntryRepository>()).Object).As<ITimeEntryRepository>();
        builder.RegisterType<CreateTimeEntryValidator>().As<IDomainValidator<CreateTimeEntryCommand>>();
        builder.RegisterType<UpdateTimeEntryValidator>().As<IDomainValidator<UpdateTimeEntryCommand>>();
        builder.RegisterType<DeleteTimeEntryValidator>().As<IDomainValidator<DeleteTimeEntryCommand>>();
        return builder.Build().Resolve<IDomainValidator<TCommand>>();
    }
}
