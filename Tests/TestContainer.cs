using Autofac;
using Domain.Interfaces;
using Domain.Validators;
using Domain.Validators.TimeEntries;
using Moq;

namespace Tests;

internal static class TestContainer
{
    public static TValidator Resolve<TValidator>(
        Mock<IEmployeeRepository>? employeeRepositoryMock = null,
        Mock<IProjectRepository>? projectRepositoryMock = null,
        Mock<IPeriodRepository>? periodRepositoryMock = null,
        Mock<ITimeEntryRepository>? timeEntryRepositoryMock = null)
        where TValidator : class
    {
        var builder = new ContainerBuilder();
        builder.RegisterInstance((employeeRepositoryMock ?? new Mock<IEmployeeRepository>()).Object).As<IEmployeeRepository>();
        builder.RegisterInstance((projectRepositoryMock ?? new Mock<IProjectRepository>()).Object).As<IProjectRepository>();
        builder.RegisterInstance((periodRepositoryMock ?? new Mock<IPeriodRepository>()).Object).As<IPeriodRepository>();
        builder.RegisterInstance((timeEntryRepositoryMock ?? new Mock<ITimeEntryRepository>()).Object).As<ITimeEntryRepository>();
        builder.RegisterType<CreateTimeEntryValidator>().As<ICreateTimeEntryValidator>();
        builder.RegisterType<UpdateTimeEntryValidator>().As<IUpdateTimeEntryValidator>();
        builder.RegisterType<DeleteTimeEntryValidator>().As<IDeleteTimeEntryValidator>();
        return builder.Build().Resolve<TValidator>();
    }
}
